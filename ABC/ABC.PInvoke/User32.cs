using System;
using System.Runtime.InteropServices;


namespace ABC.PInvoke
{
	/// <summary>
	///   Class through which user32.dll calls can be accessed for which the .NET framework offers no alternative.
	///   TODO: Clean up remaining original documentation, converting it to the wrapper's equivalents.
	/// </summary>
	public static partial class User32
	{
		const string Dll = "user32.dll";


		#region Window Functions.

		/// <summary>
		/// Calls the default window procedure to provide default processing for any window messages that an application does not process. This function ensures that every message is processed. DefWindowProc is called with the same parameters received by the window procedure.
		/// </summary>
		/// <param name="handle">A handle to the window procedure that received the message.</param>
		/// <param name="message">The message.</param>
		/// <param name="wParam">Additional message information. The content of this parameter depends on the value of the Msg parameter.</param>
		/// <param name="lParam">Additional message information. The content of this parameter depends on the value of the Msg parameter</param>
		/// <returns>The return value is the result of the message processing and depends on the message.</returns>
		[DllImport( Dll )]
		public static extern IntPtr DefWindowProc( IntPtr handle, uint message, IntPtr wParam, IntPtr lParam );

		/// <summary>
		/// Registers a specified Shell window to receive certain messages for events or notifications that are useful to Shell applications.
		/// </summary>
		/// <param name="hwnd">A handle to the window to register for Shell hook messages.</param>
		/// <returns>TRUE if the function succeeds; otherwise, FALSE.</returns>
		[DllImport( Dll, CharSet = CharSet.Auto, SetLastError = true )]
		public static extern bool RegisterShellHookWindow( IntPtr hwnd );

		/// <summary>
		/// Changes an attribute of the specified window. The function also sets the 32-bit (long) value at the specified offset into the extra window memory. Note: this function has been superseded by the SetWindowLongPtr function. To write code that is compatible with both 32-bit and 64-bit versions of Windows, use the SetWindowLongPtr function.
		/// </summary>
		/// <param name="windowHandle">A handle to the window and, indirectly, the class to which the window belongs.</param>
		/// <param name="index">The zero-based offset to the value to be set. Valid values are in the range zero through the number of bytes of extra window memory, minus the size of an integer. To set any other value, specify one of the following values.</param>
		/// <param name="dwNewLong">The replacement value.</param>
		/// <returns>If the function succeeds, the return value is the previous value of the specified 32-bit integer. If the function fails, the return value is zero. To get extended error information, call GetLastError.</returns>
		public static IntPtr SetWindowLongPtr( IntPtr windowHandle, int index, uint dwNewLong )
		{
			// GetWindowLongPtr is only supported by Win64. By checking the pointer size the correct function can be called.
			return IntPtr.Size == 8
				       ? SetWindowLongPtr64( windowHandle, index, dwNewLong )
				       : SetWindowLongPtr32( windowHandle, index, dwNewLong );
		}

		/// <summary>
		/// Retrieves a handle to the top-level window whose class name and window name match the specified strings. This function does not search child windows. This function does not perform a case-sensitive search.
		/// </summary>
		/// <param name="lpszClass">The class name or a class atom created by a previous call to the RegisterClass or RegisterClassEx function. The atom must be in the low-order word of lpClassName; the high-order word must be zero.</param>
		/// <param name="lpszWindow">The window name (the window's title). If this parameter is NULL, all window names match.</param>
		/// <returns>If the function succeeds, the return value is a handle to the window that has the specified class name and window name. If the function fails, the return value is NULL. To get extended error information, call GetLastError.</returns>
		[DllImport( Dll, CharSet = CharSet.Auto, SetLastError = true )]
		public static extern IntPtr FindWindow( string lpszClass, string lpszWindow );

		/// <summary>
		/// Retrieves a handle to the top-level window whose class name and window name match the specified strings. This function does not search child windows. This function does not perform a case-sensitive search.
		/// </summary>
		/// <param name="hwndParent">A handle to the parent window whose child windows are to be searched. If hwndParent is NULL, the function uses the desktop window as the parent window. The function searches among windows that are child windows of the desktop.></param>
		/// <param name="hwndChildAfter">A handle to a child window. The search begins with the next child window in the Z order. The child window must be a direct child window of hwndParent, not just a descendant window.</param>
		/// <param name="lpszClass">The class name or a class atom created by a previous call to the RegisterClass or RegisterClassEx function. The atom must be placed in the low-order word of lpszClass; the high-order word must be zero.</param>
		/// <param name="lpszWindow">The window name (the window's title). If this parameter is NULL, all window names match.</param>
		/// <returns>If the function succeeds, the return value is a handle to the window that has the specified class and window names. If the function fails, the return value is NULL. To get extended error information, call GetLastError.</returns>
		[DllImport( Dll, CharSet = CharSet.Auto, SetLastError = true )]
		public static extern IntPtr FindWindowEx( IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow );

		[DllImport( Dll, EntryPoint = "SetWindowLong", SetLastError = true )]
		static extern IntPtr SetWindowLongPtr32( IntPtr windowHandle, int index, uint dwNewLong );

		[DllImport( Dll, EntryPoint = "SetWindowLongPtr", SetLastError = true )]
		static extern IntPtr SetWindowLongPtr64( IntPtr windowHandle, int index, uint dwNewLong );

		#endregion // Window Functions.
	}
}