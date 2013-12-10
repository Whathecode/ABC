using System;
using System.Diagnostics;
using Whathecode.System.Windows.Interop;


namespace ABC.Windows
{
	/// <summary>
	///   Represents an application window.
	/// </summary>
	/// <remarks>This is a simple wrapper around WindowInfo in Whathecode.System.</remarks>
	/// <author>Steven Jeuris</author>
	public class Window
	{
		internal WindowInfo WindowInfo { get; private set; }


		internal Window( WindowInfo windowInfo )
		{
			WindowInfo = windowInfo;
		}

		internal Window( IntPtr windowInfo )
		{
			WindowInfo = new WindowInfo( windowInfo );
		}


		/// <summary>
		///   Retrieves the window which currently has focus.
		/// </summary>
		/// <returns>The <see cref="Window" /> of the window which currently has focus.</returns>
		public static Window GetForegroundWindow()
		{
			return new Window( WindowManager.GetForegroundWindow() );
		}

		/// <summary>
		///   Retrieves the process that created the window, when available.
		/// </summary>
		/// <returns>The process when available, null otherwise.</returns>
		public Process GetProcess()
		{
			return WindowInfo.GetProcess();
		}
	}
}
