using System;
using System.Runtime.InteropServices.Marshalling;
using System.Runtime.InteropServices;

namespace RemoteCopy.Interop
{
    [GeneratedComInterface]
    [Guid("0000000c-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    partial interface IStream : ISequentialStream
    {
        [PreserveSig]
        HRESULT Seek(long dlibMove, int dwOrigin, ref long plibNewPosition);

        [PreserveSig]
        HRESULT SetSize(long libNewSize);

        [PreserveSig]
        HRESULT CopyTo(nint pstm, long cb, ref long pcbRead, ref long pcbWritten);

        [PreserveSig]
        HRESULT Commit(uint grfCommitFlags);

        [PreserveSig]
        HRESULT Revert();

        [PreserveSig]
        HRESULT LockRegion(long libOffset, long cb, uint dwLockType);

        [PreserveSig]
        HRESULT UnlockRegion(long libOffset, long cb, uint dwLockType);

        [PreserveSig]
        HRESULT Stat(ref STATSTG pstatstg, uint grfStatFlag);

        [PreserveSig]
        HRESULT Clone(ref nint ppstm);
    }
}