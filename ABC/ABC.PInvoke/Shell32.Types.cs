using System;
using System.Runtime.InteropServices;

namespace ABC.PInvoke
{
    public partial class Shell32
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct Appbardata
        {
            public int cbSize;
            public IntPtr hWnd;
            public int uCallbackMessage;
            public int uEdge;
            public RECT rc;
            public IntPtr lParam;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        /// <summary>
        /// Get WindowLong constants
        /// </summary>
        public enum GetWindowLongConst
        {
            // ReSharper disable InconsistentNaming
            GWL_WNDPROC = (-4),
            GWL_HINSTANCE = (-6),
            GWL_HWNDPARENT = (-8),
            GWL_STYLE = (-16),
            GWL_EXSTYLE = (-20),
            GWL_USERDATA = (-21),
            GWL_ID = (-12)
            // ReSharper restore InconsistentNaming
        }
    }
}
