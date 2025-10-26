using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Threading;
using System.Threading.Tasks;
using RemoteCopy.Interop;

namespace RemoteCopy
{
    [GeneratedComClass]
    partial class ManagedDataObject : IDataObject, IAsyncOperation
    {
        bool _inOperation;
        CancellationToken _token;
        List<InnerObject> _innerObjects;

        record InnerObject(FORMATETC format, Func<nint> make);

        public Action? StartAction;
        public Action<bool>? EndAction;

        public ManagedDataObject(IEnumerable<FileSource> files, CancellationToken cancellation)
        {
            _inOperation = false;
            _innerObjects = new();
            _token = cancellation;

            // Set CFSTR_FILEDESCRIPTORW
            SetFileDescriptors(files);

            // Set n CFSTR_FILECONTENTS
            int index = 0;
            foreach (var file in files)
                AppendFileContent(index++, file.Size, file.Fetcher);
        }

        private unsafe void SetFileDescriptors(IEnumerable<FileSource> descriptors)
        {
            using var bytes = new MemoryStream();
            using var writer = new BinaryWriter(bytes);

            // Add FILEGROUPDESCRIPTOR header
            writer.Write(descriptors.Count());

            // Add N FILEDESCRIPTORs
            foreach (var file in descriptors)
            {
                // Set required fields
                var fd = new FILEDESCRIPTOR();
                fd.dwFlags = Native.FD_PROGRESSUI;

                // Set file name
                var namechars = file.Name.ToCharArray();
                Marshal.Copy(namechars, 0, (nint)fd.cFileName, namechars.Length);

                // Set optional timestamp
                if (file.Date != null)
                {
                    var modifiedAt = file.Date.Value.ToLocalTime().ToFileTime();
                    fd.dwFlags |= Native.FD_CREATETIME | Native.FD_WRITESTIME;
                    fd.ftLastWriteTime = modifiedAt;
                    fd.ftCreationTime = modifiedAt;
                }

                // Set optional length
                if (file.Size != 0)
                {
                    fd.dwFlags |= Native.FD_FILESIZE;
                    fd.nFileSizeHigh = (uint)(file.Size >> 32);
                    fd.nFileSizeLow = (uint)(file.Size & 0xffffffff);
                }

                // Add structure to buffer
                writer.Write(new Span<byte>(&fd, Marshal.SizeOf<FILEDESCRIPTOR>()));
            }

            var format = new FORMATETC
            {
                cfFormat = Native.CF_FILEDESCRIPTORW,
                ptd = 0,
                dwAspect = DVASPECT.DVASPECT_CONTENT,
                lindex = -1,
                tymed = TYMED.TYMED_HGLOBAL
            };

            var data = bytes.ToArray();
            var maker = () =>
            {
                var ptr = Marshal.AllocHGlobal(data.Length);
                Marshal.Copy(data, 0, ptr, data.Length);
                return ptr;
            };

            _innerObjects.Add(new(format, maker));
        }

        private void AppendFileContent(int index, long size, Func<Task<Stream>> fetcher)
        {
            var format = new FORMATETC
            {
                cfFormat = Native.CF_FILECONTENTS,
                ptd = 0,
                dwAspect = DVASPECT.DVASPECT_CONTENT,
                lindex = index,
                tymed = TYMED.TYMED_ISTREAM
            };

            var maker = () =>
            {
                var stream = Task.Run(fetcher).Result;
                var mstream = new ManagedStream(stream, size, _token);

                return mstream.NativePointer;
            };

            _innerObjects.Add(new(format, maker));
        }

        #region IDataObject
        public HRESULT EnumFormatEtc(DATADIR dwDirection, ref nint ppenumFormatEtc)
        {
            if (dwDirection == DATADIR.DATADIR_GET)
            {
                // Create enumerator and return it
                var formats = _innerObjects.Select(d => d.format).ToArray();
                int hr = Native.SHCreateStdEnumFmtEtc(formats.Length, formats, out ppenumFormatEtc);

                return (HRESULT)hr;
            }

            ppenumFormatEtc = 0;
            return HRESULT.E_NOTIMPL;
        }

        public HRESULT QueryGetData(ref FORMATETC pformatetc)
        {
            var format = pformatetc;

            var formatMatches = _innerObjects.Where(d => d.format.cfFormat == format.cfFormat);
            if (!formatMatches.Any())
                return HRESULT.DV_E_FORMATETC;

            var tymedMatches = formatMatches.Where(d => (d.format.tymed & format.tymed) != 0);
            if (!tymedMatches.Any())
                return HRESULT.DV_E_TYMED;

            var aspectMatches = tymedMatches.Where(d => d.format.dwAspect == format.dwAspect);
            if (!aspectMatches.Any())
                return HRESULT.DV_E_DVASPECT;

            return HRESULT.S_OK;
        }

        public HRESULT GetData(ref FORMATETC pformatetcIn, ref STGMEDIUM pmedium)
        {
            var hr = QueryGetData(ref pformatetcIn);

            if (hr >= HRESULT.S_OK)
            {
                var format = pformatetcIn;

                // Find the best match
                var dataObject = _innerObjects
                    .Where(d =>
                        (d.format.cfFormat == format.cfFormat) &&
                        (d.format.dwAspect == format.dwAspect) &&
                        (0 != (d.format.tymed & format.tymed) &&
                        (d.format.lindex == format.lindex)))
                    .FirstOrDefault();

                if (dataObject != null)
                {
                    // Populate the STGMEDIUM
                    pmedium.tymed = dataObject.format.tymed;
                    pmedium.pUnkForRelease = 0;

                    try
                    {
                        pmedium.unionmember = dataObject.make();
                        hr = HRESULT.S_OK;
                    }
                    catch (Exception ex)
                    {
#if DEBUG
                        Console.WriteLine("[E] Failed to fetch data stream: {0}", ex);
#endif
                        pmedium.unionmember = 0;
                        hr = HRESULT.E_ABORT;
                    }
                }
                else
                {
                    // Couldn't find a match
                    hr = HRESULT.DV_E_FORMATETC;
                }
            }

            return hr;
        }

        public HRESULT SetData(ref FORMATETC pformatetc, ref STGMEDIUM pmedium, bool fRelease)
        {
            if (fRelease)
                Native.ReleaseStgMedium(ref pmedium);

            throw new NotImplementedException();
        }

        public HRESULT GetDataHere(ref FORMATETC pformatetc, ref STGMEDIUM pmedium)
            => throw new NotImplementedException();

        public HRESULT GetCanonicalFormatEtc(ref FORMATETC pformatectIn, ref FORMATETC pformatetcOut)
            => throw new NotImplementedException();

        public HRESULT DAdvise(ref FORMATETC pformatetc, uint advf, nint pAdvSink, ref uint pdwConnection)
            => HRESULT.OLE_E_ADVISENOTSUPPORTED;

        public HRESULT DUnadvise(uint dwConnection)
            => HRESULT.OLE_E_ADVISENOTSUPPORTED;

        public HRESULT EnumDAdvise(ref nint ppenumAdvise)
            => HRESULT.OLE_E_ADVISENOTSUPPORTED;
        #endregion

        #region IAsyncOperation
        public HRESULT SetAsyncMode(int fDoOpAsync)
        {
            return HRESULT.E_NOTIMPL;
        }

        public HRESULT GetAsyncMode(out int pfIsOpAsync)
        {
            pfIsOpAsync = Native.VARIANT_TRUE;
            return HRESULT.S_OK;
        }

        public HRESULT StartOperation(nint pbcReserved)
        {
            if (_token.IsCancellationRequested)
                return HRESULT.E_ABORT;

            _inOperation = true;
            StartAction?.Invoke();

            Clipboard.Clear();

            return HRESULT.S_OK;
        }

        public HRESULT InOperation(out int pfInAsyncOp)
        {
            pfInAsyncOp = _inOperation ? Native.VARIANT_TRUE : Native.VARIANT_FALSE;
            return HRESULT.S_OK;
        }

        public HRESULT EndOperation(HRESULT hResult, nint pbcReserved, uint dwEffects)
        {
            _inOperation = false;
            EndAction?.Invoke(hResult == 0);

            return HRESULT.S_OK;
        }
        #endregion
    }
}