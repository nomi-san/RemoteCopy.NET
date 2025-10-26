using System;
using System.IO;
using System.Runtime.InteropServices.Marshalling;
using System.Threading;
using RemoteCopy.Interop;

namespace RemoteCopy
{
    [GeneratedComClass]
    partial class ManagedStream : IStream, IDisposable
    {
        private long _length;
        private long _position;
        private Stream _stream;
        private CancellationToken _token;

        private nint _ptr = 0;

        public ManagedStream(Stream stream, long length, CancellationToken token)
        {
            _stream = stream;
            _length = length;
            _position = 0;
            _token = token;
        }

        public void Dispose()
        {
            _stream.Dispose();
            if (_ptr != 0)
            {
                unsafe
                {
                    ComInterfaceMarshaller<IStream>.Free((void*)_ptr);
                }
            }
        }

        public nint NativePointer
        {
            get
            {
                if (_ptr == 0)
                {
                    unsafe
                    {
                        _ptr = (nint)ComInterfaceMarshaller<IStream>.ConvertToUnmanaged(this);
                    }
                }
                return _ptr;
            }
        }

        public HRESULT Read(nint pv, int cb, ref int pcbRead)
        {
            if (_token.IsCancellationRequested)
                return HRESULT.E_ABORT;

            unsafe
            {
                var buf = new Span<byte>((void*)pv, cb);
                int read = _stream.Read(buf);

                _position += read;
                pcbRead = read;
            }

            return HRESULT.S_OK;
        }

        public HRESULT Seek(long dlibMove, int dwOrigin, ref long plibNewPosition)
        {
            if (_token.IsCancellationRequested)
                return HRESULT.E_ABORT;

            switch ((SeekOrigin)dwOrigin)
            {
                case SeekOrigin.Begin:
                    _position = dlibMove;
                    break;
                case SeekOrigin.Current:
                    _position = _position + dlibMove;
                    break;
                case SeekOrigin.End:
                    _position = _length + dlibMove;
                    break;
            }

            plibNewPosition = _position;
            return HRESULT.S_OK;
        }

        public HRESULT Stat(ref STATSTG pstatstg, uint grfStatFlag)
        {
            if (_token.IsCancellationRequested)
                return HRESULT.E_ABORT;

            //*pstatstg = new Interop.STATSTG();
            pstatstg.type = /*STGTY_STREAM*/2;
            pstatstg.cbSize = _length;
            pstatstg.grfMode = (uint)STGM.STGM_READ;

            return HRESULT.S_OK;
        }

        public HRESULT Clone(ref nint ppstm)
            => throw new NotImplementedException();

        public HRESULT Commit(uint grfCommitFlags)
            => throw new NotImplementedException();

        public HRESULT CopyTo(nint pstm, long cb, ref long pcbRead, ref long pcbWritten)
            => throw new NotImplementedException();

        public HRESULT LockRegion(long libOffset, long cb, uint dwLockType)
            => throw new NotImplementedException();

        public HRESULT Revert()
            => throw new NotImplementedException();

        public HRESULT SetSize(long libNewSize)
            => throw new NotImplementedException();

        public HRESULT UnlockRegion(long libOffset, long cb, uint dwLockType)
            => throw new NotImplementedException();

        public HRESULT Write(nint pv, int cb, ref int pcbWritten)
            => throw new NotImplementedException();
    }
}