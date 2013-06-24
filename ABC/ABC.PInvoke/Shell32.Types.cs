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
    }
}
