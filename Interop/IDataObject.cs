using System;
using System.Runtime.InteropServices.Marshalling;
using System.Runtime.InteropServices;

namespace RemoteCopy.Interop
{
    [GeneratedComInterface]
    [Guid("0000010E-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    partial interface IDataObject
    {
        [PreserveSig]
        HRESULT GetData(ref FORMATETC pformatetcIn, ref STGMEDIUM pmedium);

        [PreserveSig]
        HRESULT GetDataHere(ref FORMATETC pformatetc, ref STGMEDIUM pmedium);

        [PreserveSig]
        HRESULT QueryGetData(ref FORMATETC pformatetc);

        [PreserveSig]
        HRESULT GetCanonicalFormatEtc(ref FORMATETC pformatectIn, ref FORMATETC pformatetcOut);

        [PreserveSig]
        HRESULT SetData(ref FORMATETC pformatetc, ref STGMEDIUM pmedium, [MarshalAs(UnmanagedType.Bool)] bool fRelease);

        [PreserveSig]
        HRESULT EnumFormatEtc(DATADIR dwDirection, ref nint ppenumFormatEtc);

        [PreserveSig]
        HRESULT DAdvise(ref FORMATETC pformatetc, uint advf, nint pAdvSink, ref uint pdwConnection);

        [PreserveSig]
        HRESULT DUnadvise(uint dwConnection);

        [PreserveSig]
        HRESULT EnumDAdvise(ref nint ppenumAdvise);
    }
}