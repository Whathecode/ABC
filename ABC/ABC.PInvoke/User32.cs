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
		/// Defines a new window message that is guaranteed to be unique throughout the system. The message value can be used when sending or posting messages.
		/// </summary>
		/// <param name="lpString">The message to be registered.</param>
		/// <returns>If the message is successfully registered, the return value is a message identifier in the range 0xC000 through 0xFFFF. If the function fails, the return value is zero. To get extended error information, call GetLastError.</returns>
		[DllImport( Dll, CharSet = CharSet.Auto, SetLastError = true )]
		public static extern int RegisterWindowMessage( string lpString );

		/// <summary>
		///   Sends the specified message to a window or windows.
		///   The <see cref="SendMessage" /> function calls the window procedure for the specified window and does not return until the window procedure has processed the message.
		///   To send a message and return immediately, use the SendMessageCallback or SendNotifyMessage function.
		///   To post a message to a thread's message queue and return immediately, use the PostMessage or PostThreadMessage function.
		///   TODO: Use custom marshaler to make message typesafe? http://msdn.microsoft.com/en-us/library/system.runtime.interopservices.icustommarshaler.aspx
		/// </summary>
		/// <param name="windowHandle">
		///   A handle to the window whose window procedure will receive the message.
		///   If this parameter is HWND_BROADCAST ((HWND)0xffff), the message is sent to all top-level windows in the system, including disabled or invisible unowned windows,
		///   overlapped windows, and pop-up windows; but the message is not sent to child windows.
		///   Message sending is subject to UIPI. The thread of a process can send messages only to message queues of threads in processes of lesser or equal integrity level.
		/// </param>
		/// <param name="message">The message to be sent. For lists of the system-provided messages, see System-Defined Messages.</param>
		/// <param name="wParam">Additional message-specific information.</param>
		/// <param name="lParam">Additional message-specific information.</param>
		/// <returns>The return value specifies the result of the message processing; it depends on the message sent.</returns>
		/// <remarks>
		///   When a message is blocked by UIPI the last error, retrieved with GetLastWin32Error, is set to 5 (access denied).
		///   Applications that need to communicate using HWND_BROADCAST should use the <see cref="RegisterWindowMessage" /> function to obtain a unique message for inter-application communication.
		///   The system only does marshalling for system messages (those in the range 0 to (WM_USER-1)). To send other messages (those >= WM_USER) to another process, you must do custom marshalling.
		///   If the specified window was created by the calling thread, the window procedure is called immediately as a subroutine.
		///   If the specified window was created by a different thread, the system switches to that thread and calls the appropriate window procedure.
		///   Messages sent between threads are processed only when the receiving thread executes message retrieval code.
		///   The sending thread is blocked until the receiving thread processes the message.
		///   However, the sending thread will process incoming nonqueued messages while waiting for its message to be processed.
		///   To prevent this, use SendMessageTimeout with SMTO_BLOCK set. For more information on nonqueued messages, see Nonqueued Messages.
		///   An accessibility application can use SendMessage to send WM_APPCOMMAND messages to the shell to launch applications.
		///   This functionality is not guaranteed to work for other types of applications.
		/// </remarks>
		[DllImport( Dll, SetLastError = true )]
		public static extern IntPtr SendMessage( IntPtr windowHandle, uint message, IntPtr wParam, IntPtr lParam );

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

		[DllImport( Dll, EntryPoint = "SetWindowLong", SetLastError = true )]
		static extern IntPtr SetWindowLongPtr32( IntPtr windowHandle, int index, uint dwNewLong );

		[DllImport( Dll, EntryPoint = "SetWindowLongPtr", SetLastError = true )]
		static extern IntPtr SetWindowLongPtr64( IntPtr windowHandle, int index, uint dwNewLong );

		#endregion // Window Functions.


		#region Hooks

		/// <summary>
		/// Passes the hook information to the next hook procedure in the current hook chain. A hook procedure can call this function either before or after processing the hook information.
		/// </summary>
		/// <param name="hook">This parameter is ignored.</param>
		/// <param name="hookCode">The hook code passed to the current hook procedure. The next hook procedure uses this code to determine how to process the hook information.</param>
		/// <param name="wParam">The wParam value passed to the current hook procedure. The meaning of this parameter depends on the type of hook associated with the current hook chain.</param>
		/// <param name="lParam">The lParam value passed to the current hook procedure. The meaning of this parameter depends on the type of hook associated with the current hook chain.</param>
		/// <returns>This value is returned by the next hook procedure in the chain. The current hook procedure must also return this value. The meaning of the return value depends on the hook type. For more information, see the descriptions of the individual hook procedures.</returns>
		[DllImport( Dll, CharSet = CharSet.Auto, SetLastError = true )]
		public static extern int CallNextHookEx( IntPtr hook, int hookCode, IntPtr wParam, IntPtr lParam );

		/// <summary>
		/// Installs an application-defined hook procedure into a hook chain. You would install a hook procedure to monitor the system for certain types of events. These events are associated either with a specific thread or with all threads in the same desktop as the calling thread.
		/// </summary>
		/// <param name="hook">The type of hook procedure to be installed. This parameter can be one of the following values.</param>
		/// <param name="hookProcedure">A pointer to the hook procedure. If the dwThreadId parameter is zero or specifies the identifier of a thread created by a different process, the lpfn parameter must point to a hook procedure in a DLL. Otherwise, lpfn can point to a hook procedure in the code associated with the current process.</param>
		/// <param name="instance">A handle to the DLL containing the hook procedure pointed to by the lpfn parameter. The hMod parameter must be set to NULL if the dwThreadId parameter specifies a thread created by the current process and if the hook procedure is within the code associated with the current process.</param>
		/// <param name="threadId">he identifier of the thread with which the hook procedure is to be associated. For desktop apps, if this parameter is zero, the hook procedure is associated with all existing threads running in the same desktop as the calling thread. For Windows Store apps, see the Remarks section.</param>
		/// <returns></returns>
		[DllImport( Dll, CharSet = CharSet.Auto, SetLastError = true )]
		public static extern IntPtr SetWindowsHookEx( int hook, Delegate hookProcedure, IntPtr instance, int threadId );

		/// <summary>
		/// Removes a hook procedure installed in a hook chain by the SetWindowsHookEx function.
		/// </summary>
		/// <param name="hookHandle">A handle to the hook to be removed. This parameter is a hook handle obtained by a previous call to SetWindowsHookEx.</param>
		/// <returns>
		/// If the function succeeds, the return value is nonzero.
		/// If the function fails, the return value is zero. To get extended error information, call GetLastError.
		/// </returns>
		[DllImport( Dll, CharSet = CharSet.Auto, SetLastError = true )]
		public static extern bool UnhookWindowsHookEx( IntPtr hookHandle );

		/// <summary>
		/// Frees a hot key previously registered by the calling thread.
		/// </summary>
		/// <param name="handle">A handle to the window associated with the hot key to be freed. This parameter should be NULL if the hot key is not associated with a window.</param>
		/// <param name="id">The identifier of the hot key to be freed</param>
		/// <returns>
		/// If the function succeeds, the return value is nonzero.
		/// If the function fails, the return value is zero. To get extended error information, call GetLastError.
		/// </returns>
		[DllImport( Dll, CharSet = CharSet.Auto, SetLastError = true )]
		public static extern bool UnregisterHotKey( IntPtr handle, int id );

		/// <summary>
		/// Defines a system-wide hot key.
		/// </summary>
		/// <param name="handle">A handle to the window that will receive WM_HOTKEY messages generated by the hot key. If this parameter is NULL, WM_HOTKEY messages are posted to the message queue of the calling thread and must be processed in the message loop.</param>
		/// <param name="id">The identifier of the hot key. If the hWnd parameter is NULL, then the hot key is associated with the current thread rather than with a particular window. If a hot key already exists with the same hWnd and id parameters, see Remarks for the action taken.</param>
		/// <param name="accessKey">The keys that must be pressed in combination with the key specified by the uVirtKey parameter in order to generate the WM_HOTKEY message. The fsModifiers parameter can be a combination of the following values.</param>
		/// <param name="keys">The virtual-key code of the hot key. See Virtual Key Codes.</param>
		/// <returns></returns>
		[DllImport( Dll, CharSet = CharSet.Auto, SetLastError = true )]
		public static extern bool RegisterHotKey( IntPtr handle, int id, uint accessKey, uint keys );

		#endregion
	}
}