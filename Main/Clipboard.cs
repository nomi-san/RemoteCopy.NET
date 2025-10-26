using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using RemoteCopy.Interop;

namespace RemoteCopy
{
    public static class Clipboard
    {
        private const int OleRetryCount = 10;
        private const int OleRetryDelay = 100;
        private const int OleFlushDelay = 10;

        [ThreadStatic]
        private static bool OleInitialized = false;

        private static void CheckOleAccess()
        {
            if (!OleInitialized)
            {
                int hr = Native.OleInitialize(0);
                if (hr >= 0)
                    OleInitialized = true;
                else
                    throw new COMException("OleInitialize fails.", hr);
            }
        }

        sealed class Disposable : IDisposable
        {
            private volatile Action? _dispose;
            public Disposable(Action dispose)
            {
                _dispose = dispose;
            }
            public bool IsDisposed => _dispose == null;
            public void Dispose()
            {
                Interlocked.Exchange(ref _dispose, null)?.Invoke();
            }
        }

        private static IDisposable OpenClipboard()
        {
            var i = OleRetryCount;

            while (!Native.OpenClipboard(0))
            {
                if (--i == 0)
                    throw new TimeoutException("Timeout opening clipboard.");
                Thread.Sleep(100);
            }

            return new Disposable(static () => Native.CloseClipboard());
        }

        private static async Task<IDisposable> OpenClipboardAsync()
        {
            var i = OleRetryCount;

            while (!Native.OpenClipboard(0))
            {
                if (--i == 0)
                    throw new TimeoutException("Timeout opening clipboard.");
                await Task.Delay(100);
            }

            return new Disposable(static () => Native.CloseClipboard());
        }

        public static uint SequenceNumber
        {
            get => Native.GetClipboardSequenceNumber();
        }

        public static bool HasText()
        {
            return Native.IsClipboardFormatAvailable(Native.CF_UNICODETEXT);
        }

        public static bool HasFiles()
        {
            return Native.IsClipboardFormatAvailable(Native.CF_HDROP);
        }

        public static string? GetText()
        {
            using (OpenClipboard())
            {
                IntPtr hText = Native.GetClipboardData(Native.CF_UNICODETEXT);
                if (hText == IntPtr.Zero)
                    return null;

                var pText = Native.GlobalLock(hText);
                if (pText == IntPtr.Zero)
                    return null;

                var rv = Marshal.PtrToStringUni(pText);
                Native.GlobalUnlock(hText);
                return rv;
            }
        }

        public static async Task<string?> GetTextAsync()
        {
            using (await OpenClipboardAsync())
            {
                IntPtr hText = Native.GetClipboardData(Native.CF_UNICODETEXT);
                if (hText == IntPtr.Zero)
                    return null;

                var pText = Native.GlobalLock(hText);
                if (pText == IntPtr.Zero)
                    return null;

                var rv = Marshal.PtrToStringUni(pText);
                Native.GlobalUnlock(hText);
                return rv;
            }
        }

        public static List<string> GetFiles()
        {
            var files = new List<string>();

            using (OpenClipboard())
            {
                var hData = Native.GetClipboardData(Native.CF_HDROP);
                if (hData != IntPtr.Zero)
                {
                    var hDrop = Native.GlobalLock(hData);
                    if (hDrop != IntPtr.Zero)
                        GetDropFiles(hDrop, files);

                    Native.GlobalUnlock(hData);
                }
            }

            return files;
        }

        public static async Task<List<string>> GetFilesAsync()
        {
            var files = new List<string>();

            using (await OpenClipboardAsync())
            {
                var hData = Native.GetClipboardData(Native.CF_HDROP);
                if (hData != IntPtr.Zero)
                {
                    var hDrop = Native.GlobalLock(hData);
                    if (hDrop != IntPtr.Zero)
                        GetDropFiles(hDrop, files);

                    Native.GlobalUnlock(hData);
                }
            }

            return files;
        }

        private static unsafe int GetDropFiles(nint hdrop, List<string> files)
        {
            int count = Native.DragQueryFile(hdrop, -1, null, 0);

            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    int pathLen = Native.DragQueryFile(hdrop, i, null, 0);
                    var namebuf = new char[pathLen + 1];

                    pathLen = Native.DragQueryFile(hdrop, i, namebuf, namebuf.Length);
                    if (pathLen > 0)
                        files.Add(new string(namebuf, 0, pathLen));
                }
            }

            return count;
        }

        public static void SetText(string? text)
        {
            using (OpenClipboard())
            {
                Native.EmptyClipboard();

                if (text is not null)
                {
                    var hGlobal = Marshal.StringToHGlobalUni(text);
                    Native.SetClipboardData(Native.CF_UNICODETEXT, hGlobal);
                }
            }
        }

        public static async Task SetTextAsync(string? text)
        {
            using (await OpenClipboardAsync())
            {
                Native.EmptyClipboard();

                if (text is not null)
                {
                    var hGlobal = Marshal.StringToHGlobalUni(text);
                    Native.SetClipboardData(Native.CF_UNICODETEXT, hGlobal);
                }
            }
        }

        public static void SetFiles(IEnumerable<FileSource> files, CancellationToken cancellation, Action? onBegin = null, Action<bool>? onEnd = null)
        {
            CheckOleAccess();
            int i = OleRetryCount;

            var data = new ManagedDataObject(files, cancellation);
            data.StartAction = onBegin;
            data.EndAction = onEnd;

            while (true)
            {
                int hr = Native.OleSetClipboard(data);

                if (hr == 0)
                    break;

                if (--i == 0)
                    Marshal.ThrowExceptionForHR(hr);

                Thread.Sleep(OleRetryDelay);
            }
        }

        public static async Task SetFilesAsync(IEnumerable<FileSource> files, CancellationToken cancellation, Action? onBegin = null, Action<bool>? onEnd = null)
        {
            CheckOleAccess();
            int i = OleRetryCount;

            var data = new ManagedDataObject(files, cancellation);
            data.StartAction = onBegin;
            data.EndAction = onEnd;

            while (true)
            {
                int hr = Native.OleSetClipboard(data);

                if (hr == 0)
                    break;

                if (--i == 0)
                    Marshal.ThrowExceptionForHR(hr);

                await Task.Delay(OleRetryDelay);
            }
        }

        public static void Clear()
        {
            using (OpenClipboard())
            {
                Native.EmptyClipboard();
            }
        }

        public static async Task ClearAsync()
        {
            using (await OpenClipboardAsync())
            {
                Native.EmptyClipboard();
            }
        }

        public static void Flush()
        {
            CheckOleAccess();
            Thread.Sleep(OleFlushDelay);

            int i = OleRetryCount;

            while (true)
            {
                int hr = Native.OleFlushClipboard();

                if (hr == 0)
                {
                    break;
                }

                if (--i == 0)
                {
                    Marshal.ThrowExceptionForHR(hr);
                }

                Thread.Sleep(OleRetryDelay);
            }
        }

        public static async Task FlushAsync()
        {
            CheckOleAccess();
            await Task.Delay(OleFlushDelay);

            int i = OleRetryCount;

            while (true)
            {
                int hr = Native.OleFlushClipboard();

                if (hr == 0)
                {
                    break;
                }

                if (--i == 0)
                {
                    Marshal.ThrowExceptionForHR(hr);
                }

                await Task.Delay(OleRetryDelay);
            }
        }
    }
}