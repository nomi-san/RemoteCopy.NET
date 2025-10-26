using System;
using System.Runtime.InteropServices.Marshalling;
using System.Runtime.InteropServices;

namespace RemoteCopy.Interop
{
    [GeneratedComInterface]
    [Guid("0C733A30-2A1C-11CE-ADE5-00AA0044773D")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    partial interface ISequentialStream
    {
        [PreserveSig]
        HRESULT Read(nint pv, int cb, ref int pcbRead);

        [PreserveSig]
        HRESULT Write(nint pv, int cb, ref int pcbWritten);
    }
}