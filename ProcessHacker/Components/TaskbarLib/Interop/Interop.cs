using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace TaskbarLib.Interop
{
    [Flags]
    internal enum KnownDestCategory
    {
        FREQUENT = 1,
        RECENT
    }
   
    [Flags]
    internal enum AppDocListType
    {
        ADLT_RECENT = 0,
        ADLT_FREQUENT
    }
   
    [StructLayout(LayoutKind.Sequential)]
    internal struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;

        public RECT(int left, int top, int right, int bottom)
        {
            this.left = left;
            this.top = top;
            this.right = right;
            this.bottom = bottom;
        }
    }
   
    [Flags]
    internal enum TaskBarProgressFlag
    {
        NoProgress = 0,
        Indeterminate = 0x1,
        Normal = 0x2,
        Error = 0x4,
        Paused = 0x8
    }
   
    [Flags]
    internal enum TBATFLAG
    {
        TBATF_USEMDITHUMBNAIL = 0x1,
        TBATF_USEMDILIVEPREVIEW = 0x2
    }
  
    [Flags]
    internal enum ThumbnailButtonMask
    {
        Bitmap = 0x1,
        Icon = 0x2,
        Tooltip = 0x4,
        Flags = 0x8
    }
   
    [Flags]
    internal enum ThumbnailButtonFlags
    {
        ENABLED = 0,
        DISABLED = 0x1,
        DISMISSONCLICK = 0x2,
        NOBACKGROUND = 0x4,
        HIDDEN = 0x8
    }
    [Flags]
    internal enum SIGDN : uint
    {
        SIGDN_NORMALDISPLAY = 0x00000000,           // SHGDN_NORMAL
        SIGDN_PARENTRELATIVEPARSING = 0x80018001,   // SHGDN_INFOLDER | SHGDN_FORPARSING
        SIGDN_DESKTOPABSOLUTEPARSING = 0x80028000,  // SHGDN_FORPARSING
        SIGDN_PARENTRELATIVEEDITING = 0x80031001,   // SHGDN_INFOLDER | SHGDN_FOREDITING
        SIGDN_DESKTOPABSOLUTEEDITING = 0x8004c000,  // SHGDN_FORPARSING | SHGDN_FORADDRESSBAR
        SIGDN_FILESYSPATH = 0x80058000,             // SHGDN_FORPARSING
        SIGDN_URL = 0x80068000,                     // SHGDN_FORPARSING
        SIGDN_PARENTRELATIVEFORADDRESSBAR = 0x8007c001,     // SHGDN_INFOLDER | SHGDN_FORPARSING | SHGDN_FORADDRESSBAR
        SIGDN_PARENTRELATIVE = 0x80080001           // SHGDN_INFOLDER
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct POINT
    {
        internal int X;
        internal int Y;

        internal POINT(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    internal struct THUMBBUTTON
    {
        [MarshalAs(UnmanagedType.U4)]
        public ThumbnailButtonMask dwMask;
        public uint iId;
        public uint iBitmap;
        public IntPtr hIcon;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szTip;
        [MarshalAs(UnmanagedType.U4)]
        public ThumbnailButtonFlags dwFlags;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    internal struct PropertyKey
    {
        public Guid fmtid;
        public uint pid;

        public PropertyKey(Guid fmtid, uint pid)
        {
            this.fmtid = fmtid;
            this.pid = pid;
        }

        public static PropertyKey PKEY_Title = new PropertyKey(new Guid("F29F85E0-4FF9-1068-AB91-08002B27B3D9"), 2);
        public static PropertyKey PKEY_AppUserModel_ID = new PropertyKey(new Guid("9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3"), 5);
        public static PropertyKey PKEY_AppUserModel_IsDestListSeparator = new PropertyKey(new Guid("9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3"), 6);
        public static PropertyKey PKEY_AppUserModel_RelaunchCommand = new PropertyKey(new Guid("9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3"), 2);
        public static PropertyKey PKEY_AppUserModel_RelaunchDisplayNameResource = new PropertyKey(new Guid("9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3"), 4);
        public static PropertyKey PKEY_AppUserModel_RelaunchIconResource = new PropertyKey(new Guid("9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3"), 3);
    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct CALPWSTR
    {
        [FieldOffset(0)]
        internal uint cElems;
        [FieldOffset(4)]
        internal IntPtr pElems;
    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct PropVariant
    {
        [FieldOffset(0)]
        private ushort vt;
        [FieldOffset(8)]
        private IntPtr pointerValue;
        [FieldOffset(8)]
        private byte byteValue;
        [FieldOffset(8)]
        private long longValue;
        [FieldOffset(8)]
        private short boolValue;
        [MarshalAs(UnmanagedType.Struct)]
        [FieldOffset(8)]
        private CALPWSTR calpwstr;

        [DllImport("ole32.dll")]
        private static extern int PropVariantClear(ref PropVariant pvar);

        public VarEnum VarType
        {
            get { return (VarEnum)vt; }
        }

        public void SetValue(String val)
        {
            this.Clear();
            this.vt = (ushort)VarEnum.VT_LPWSTR;
            this.pointerValue = Marshal.StringToCoTaskMemUni(val);
        }
        public void SetValue(bool val)
        {
            this.Clear();
            this.vt = (ushort)VarEnum.VT_BOOL;
            this.boolValue = val ? (short)-1 : (short)0;
        }

        public string GetValue()
        {
            return Marshal.PtrToStringUni(this.pointerValue);
        }

        public void Clear()
        {
            PropVariantClear(ref this);
        }
    }

    [SuppressUnmanagedCodeSecurity]
    internal static class SafeNativeMethods
    {
        //Obviously, these GUIDs shouldn't be modified.  The reason they
        //are not readonly is that they are passed with 'ref' to various
        //native methods.
        public static Guid IID_IObjectArray = new Guid("92CA9DCD-5622-4BBA-A805-5E9F541BD8C9");
        public const string IID_IObjectCollection = "5632B1A4-E38A-400A-928A-D4CD63230295";
        public static Guid IID_IPropertyStore = new Guid("886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99");
        public static Guid IID_IUnknown = new Guid("00000000-0000-0000-C000-000000000046");

        public const int DWM_SIT_DISPLAYFRAME = 0x00000001;
        public const int DWMWA_FORCE_ICONIC_REPRESENTATION = 7;
        public const int DWMWA_HAS_ICONIC_BITMAP = 10;

        public const int WA_ACTIVE = 1;
        public const int WA_CLICKACTIVE = 2;

        public const int SC_CLOSE = 0xF060;

        // Thumbbutton WM_COMMAND notification
        public const uint THBN_CLICKED = 0x1800;
    }
    [SuppressUnmanagedCodeSecurity]
    internal static class UnsafeNativeMethods
    {
        public static readonly uint WM_TaskbarButtonCreated = RegisterWindowMessage("TaskbarButtonCreated");

        [DllImport("shell32.dll")]
        public static extern int SHGetPropertyStoreForWindow(IntPtr hwnd, ref Guid iid /*IID_IPropertyStore*/, [Out(), MarshalAs(UnmanagedType.Interface)] out IPropertyStore propertyStore);
       
        [DllImport("dwmapi.dll")]
        public static extern int DwmSetIconicThumbnail(IntPtr hwnd, IntPtr hbitmap, uint flags);
       
        [DllImport("dwmapi.dll")]
        public static extern int DwmSetIconicLivePreviewBitmap(IntPtr hwnd, IntPtr hbitmap, ref POINT ptClient, uint flags);
       
        [DllImport("dwmapi.dll")]
        public static extern int DwmSetIconicLivePreviewBitmap(IntPtr hwnd, IntPtr hbitmap, IntPtr ptClient, uint flags);
       
        [DllImport("dwmApi.dll")]
        internal static extern int DwmSetWindowAttribute(IntPtr hwnd, uint dwAttributeToSet, IntPtr pvAttributeValue, uint cbAttribute);
       
        [DllImport("dwmApi.dll")]
        internal static extern int DwmSetWindowAttribute(IntPtr hwnd, uint dwAttributeToSet, ref int pvAttributeValue, uint cbAttribute);
      
        [DllImport("dwmapi.dll")]
        public static extern int DwmInvalidateIconicBitmaps(IntPtr hwnd);
       
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern uint RegisterWindowMessage(string lpString);

        

        [DllImport("shell32.dll")]
        public static extern void SetCurrentProcessExplicitAppUserModelID([MarshalAs(UnmanagedType.LPWStr)] string AppID);
       
        [DllImport("shell32.dll")]
        public static extern void GetCurrentProcessExplicitAppUserModelID([Out(), MarshalAs(UnmanagedType.LPWStr)] out string AppID);
       
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hwnd, ref RECT rect);
       
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetClientRect(IntPtr hwnd, ref RECT rect);
        public static bool GetClientSize(IntPtr hwnd, out System.Drawing.Size size)
        {
            RECT rect = new RECT();
            if (!GetClientRect(hwnd, ref rect))
            {
                size = new System.Drawing.Size(-1, -1);
                return false;
            }
            size = new System.Drawing.Size(rect.right, rect.bottom);
            return true;
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ShowWindow(IntPtr hwnd, int cmd);
      
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(IntPtr hwnd, IntPtr hwndInsertAfter, int X, int Y, int cx, int cy, uint flags);
      
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ClientToScreen(IntPtr hwnd, ref POINT point);
       
        [DllImport("user32.dll")]
        public static extern int GetWindowText(IntPtr hwnd, StringBuilder str, int maxCount);
       
        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool BitBlt (IntPtr hDestDC, int destX, int destY, int width, int height, IntPtr hSrcDC, int srcX, int srcY, uint operation);
        
        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool StretchBlt(IntPtr hDestDC, int destX, int destY, int destWidth, int destHeight, IntPtr hSrcDC, int srcX, int srcY, int srcWidth, int srcHeight, uint operation);
        
        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowDC(IntPtr hwnd);
       
        [DllImport("user32.dll")]
        public static extern int ReleaseDC(IntPtr hwnd, IntPtr hdc);
       
        [DllImport("Shell32", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern uint SHCreateItemFromParsingName([MarshalAs(UnmanagedType.LPWStr)] string path, /* The following parameter is not used - binding context. */ IntPtr pbc, ref Guid riid, [MarshalAs(UnmanagedType.Interface)] out IShellItem shellItem);
    }
}