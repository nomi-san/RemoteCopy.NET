using System;
using System.Runtime.InteropServices;

namespace RemoteCopy.Interop
{
    static partial class Native
    {
        public const short VARIANT_FALSE = 0;
        public const short VARIANT_TRUE = -1;

        public const uint FD_CREATETIME = 0x00000008;
        public const uint FD_WRITESTIME = 0x00000020;
        public const uint FD_FILESIZE = 0x00000040;
        public const uint FD_PROGRESSUI = 0x00004000;

        public const ushort CF_UNICODETEXT = 13;
        public const ushort CF_HDROP = 15;

        public static ushort CF_FILECONTENTS = RegisterClipboardFormatA("FileContents");
        public static ushort CF_FILEDESCRIPTORW = RegisterClipboardFormatA("FileGroupDescriptorW");
        public static ushort CF_PASTESUCCEEDED = RegisterClipboardFormatA("Paste Succeeded");
        public static ushort CF_PERFORMEDDROPEFFECT = RegisterClipboardFormatA("Performed DropEffect");
        public static ushort CF_PREFERREDDROPEFFECT = RegisterClipboardFormatA("Preferred DropEffect");

        [LibraryImport("user32.dll", StringMarshalling = StringMarshalling.Utf8)]
        private static partial ushort RegisterClipboardFormatA(string format);

        [LibraryImport("user32", StringMarshalling = StringMarshalling.Utf16)]
        private static unsafe partial int GetClipboardFormatNameW(uint format, char* lpszFormatName, int cchMaxCount);

        public static unsafe string GetClipboardFormatName(ushort fmt)
        {
            const int capacity = 256;
            var clipboardFormatName = stackalloc char[capacity];

            int numberOfChars = GetClipboardFormatNameW(fmt, clipboardFormatName, capacity);
            if (numberOfChars <= 0)
                return "<unk_name>";

            return new string(clipboardFormatName, 0, numberOfChars);
        }

        [LibraryImport("ole32.dll")]
        public static partial int OleInitialize(nint reserved);

        [LibraryImport("ole32.dll")]
        public static partial int OleSetClipboard([MarshalAs(UnmanagedType.Interface)] IDataObject data);

        [LibraryImport("ole32.dll")]
        public static partial int OleFlushClipboard();

        [LibraryImport("ole32.dll")]
        public static partial void ReleaseStgMedium(ref STGMEDIUM medium);

        [LibraryImport("shell32.dll")]
        public static unsafe partial int SHCreateStdEnumFmtEtc(int cfmt, FORMATETC[] afmt, out nint pEnumFmt);

        [LibraryImport("kernel32.dll")]
        public static partial IntPtr GlobalLock(IntPtr hMem);

        [LibraryImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool GlobalUnlock(IntPtr hMem);

        [LibraryImport("kernel32.dll")]
        public static partial nint GlobalSize(IntPtr handle);

        public static bool SUCCEEDED(int hr) => (0 <= hr);

        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool OpenClipboard(nint hWndNewOwner);

        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool CloseClipboard();

        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool IsClipboardFormatAvailable(uint uFormat);

        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool EmptyClipboard();

        [LibraryImport("user32.dll")]
        public static partial nint GetClipboardData(uint uFormat);

        [LibraryImport("user32.dll")]
        public static partial nint SetClipboardData(uint uFormat, nint hMemGlobal);

        [LibraryImport("user32.dll")]
        public static partial uint GetClipboardSequenceNumber();

        [LibraryImport("shell32.dll", EntryPoint = "DragQueryFileW", StringMarshalling = StringMarshalling.Utf16)]
        public static partial int DragQueryFile(IntPtr hDrop, int iFile, [Out] char[]? lpszFile, int cch);
    }
}