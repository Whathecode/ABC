using System;
using System.Runtime.InteropServices;
using Interop = Whathecode.Interop.User32;


namespace ABC.PInvoke
{
	public partial class User32
	{
		/// <summary>
		/// Contains information about a mouse event passed to a WH_MOUSE hook procedure, MouseProc.
		/// </summary>
		[StructLayout( LayoutKind.Sequential )]
		public struct MouseHookStruct
		{
			public Interop.Point pt;
			public int hwnd;
			public int wHitTestCode;
			public int dwExtraInfo;
		}

		/// <summary>
		/// Specifies or receives the attributes of a list-view item. This structure has been updated to support a new mask value (LVIF_INDENT) that enables item indenting. This structure supersedes the LV_ITEM structure.
		/// </summary>
		[StructLayout( LayoutKind.Sequential )]
		public struct Lvitem
		{
			public int Mask;
			public int Item;
			public int SubItem;
			public int State;
			public int StateMask;
			public IntPtr PszText; // string
			public int CchTextMax;
			public int Image;
			public IntPtr LParam;
			public int Indent;
			public int GroupId;
			public int CColumns;
			public IntPtr PuColumns;
		}


		#region Window types

		/// <summary>
		///   TODO: Incomplete list. Is this the complete one?: http://msdn.microsoft.com/en-us/library/windows/desktop/ff468922(v=vs.85).aspx
		/// </summary>
		public enum WindowNotification : uint
		{
			/// <summary>
			///   Sent as a signal that a window or an application should terminate. A window receives this message through its WindowProc function.
			///   If an application processes this message, it should return zero.
			/// </summary>
			/// <remarks>
			///   An application can prompt the user for confirmation, prior to destroying a window,
			///   by processing the <see cref="Close" /> message and calling the DestroyWindow function only if the user confirms the choice.
			///   By default, the DefWindowProc function calls the DestroyWindow function to destroy the window.
			/// </remarks>
			Close = 0x0010
		}

		/// <summary>
		/// ListView API Constants
		/// </summary>
		public const uint LVM_FIRST = 0x1000;

		public const uint LVM_GETITEMCOUNT = LVM_FIRST + 4;
		public const uint LVM_GETITEMW = LVM_FIRST + 75;
		public const uint LVM_SETITEMPOSITION = 0x1000 + 15;
		public const uint LVM_GETITEMPOSITION = LVM_FIRST + 16;

		public const int LVIF_TEXT = 0x0001;

		#endregion // Window types
	}
}