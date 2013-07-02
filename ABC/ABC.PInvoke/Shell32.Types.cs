using System;
using System.Runtime.InteropServices;


namespace ABC.PInvoke
{
	public partial class Shell32
	{
		[StructLayout( LayoutKind.Sequential )]
		public struct Appbardata
		{
			public int cbSize;
			public IntPtr hWnd;
			public int uCallbackMessage;
			public int uEdge;
			public WinDef.Rectangle rc;
			public IntPtr lParam;
		}

		/// <summary>
		/// Get WindowLong constants
		/// </summary>
		public enum GetWindowLongConst
		{
			GWL_WNDPROC = ( -4 ),
			GWL_HINSTANCE = ( -6 ),
			GWL_HWNDPARENT = ( -8 ),
			GWL_STYLE = ( -16 ),
			GWL_EXSTYLE = ( -20 ),
			GWL_USERDATA = ( -21 ),
			GWL_ID = ( -12 )
		}

		public enum AppBarMessages
		{
			ABM_NEW = 0,
			ABM_REMOVE,
			ABM_QUERYPOS,
			ABM_SETPOS,
			ABM_GETSTATE,
			ABM_GETTASKBARPOS,
			ABM_ACTIVATE,
			ABM_GETAUTOHIDEBAR,
			ABM_SETAUTOHIDEBAR,
			ABM_WINDOWPOSCHANGED,
			ABM_SETSTATE
		}

		public enum AppBarNotifications
		{
			ABN_STATECHANGE = 0,
			ABN_POSCHANGED,
			ABN_FULLSCREENAPP,
			ABN_WINDOWARRANGE
		}
	}
}