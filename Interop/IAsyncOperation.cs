using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace RemoteCopy.Interop
{
    [GeneratedComInterface]
    [Guid("3D8B0590-F691-11d2-8EA9-006097DF5BD4")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    partial interface IAsyncOperation
    {
        [PreserveSig]
        HRESULT SetAsyncMode(int fDoOpAsync);
        
        [PreserveSig]
        HRESULT GetAsyncMode(out int pfIsOpAsync);
        
        [PreserveSig]
        HRESULT StartOperation(nint pbcReserved);
        
        [PreserveSig]
        HRESULT InOperation(out int pfInAsyncOp);
        
        [PreserveSig]
        HRESULT EndOperation(HRESULT hResult, nint pbcReserved, uint dwEffects);
    }
}