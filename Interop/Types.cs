using System;
using System.Runtime.InteropServices;

namespace RemoteCopy.Interop
{
    enum HRESULT : int
    {
        S_OK = 0,
        S_FALSE = 1,

        E_FAIL = unchecked((int)0x80004005),
        E_ABORT = unchecked((int)0x80004004),
        E_UNEXPECTED = unchecked((int)0x8000FFFF),
        E_PENDING = unchecked((int)0x8000000A),
        E_NOTIMPL = unchecked((int)0x80004001),

        DRAGDROP_S_DROP = 0x00040100,
        DRAGDROP_S_CANCEL = 0x00040101,
        DRAGDROP_S_USEDEFAULTCURSORS = 0x00040102,
        DV_E_DVASPECT = -2147221397,
        DV_E_FORMATETC = -2147221404,
        DV_E_TYMED = -2147221399,
        FD_CREATETIME = 0x00000008,
        FD_WRITESTIME = 0x00000020,
        FD_FILESIZE = 0x00000040,
        FD_PROGRESSUI = 0x00004000,
        OLE_E_ADVISENOTSUPPORTED = -2147221501,
    }

    [Flags]
    enum TYMED
    {
        TYMED_NULL = 0,
        TYMED_HGLOBAL = 1,
        TYMED_FILE = 2,
        TYMED_ISTREAM = 4,
        TYMED_ISTORAGE = 8,
        TYMED_GDI = 16,
        TYMED_MFPICT = 32,
        TYMED_ENHMF = 64
    }

    [Flags]
    enum DVASPECT
    {
        DVASPECT_CONTENT = 1,
        DVASPECT_THUMBNAIL = 2,
        DVASPECT_ICON = 4,
        DVASPECT_DOCPRINT = 8,
    }

    [Flags]
    enum STGM
    {
        STGM_READ = 0x00000000,
        STGM_WRITE = 0x00000001,
        STGM_READWRITE = 0x00000002,
    }

    enum DATADIR
    {
        DATADIR_GET = 1,
        DATADIR_SET = 2,
    }

    [StructLayout(LayoutKind.Sequential)]
    struct STGMEDIUM
    {
        public TYMED tymed;
        public IntPtr unionmember;
        public IntPtr pUnkForRelease;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct FORMATETC
    {
        public ushort cfFormat;
        public IntPtr ptd;
        public DVASPECT dwAspect;
        public int lindex;
        public TYMED tymed;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct STATSTG
    {
        public nint pwcsName;
        public uint type;
        public long cbSize;
        public ulong mtime;
        public ulong ctime;
        public ulong atime;
        public uint grfMode;
        public uint grfLocksSupported;
        public Guid clsid;
        public uint grfStateBits;
        public uint reserved;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct FILEGROUPDESCRIPTOR
    {
        public int cItems;
        // Followed by 0 or more FILEDESCRIPTORs
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    struct FILEDESCRIPTOR
    {
        public uint dwFlags;
        public Guid clsid;
        public int sizelcx;
        public int sizelcy;
        public int pointlx;
        public int pointly;
        public uint dwFileAttributes;
        public long ftCreationTime;
        public long ftLastAccessTime;
        public long ftLastWriteTime;
        public uint nFileSizeHigh;
        public uint nFileSizeLow;
        public unsafe fixed char cFileName[260];
    }
}