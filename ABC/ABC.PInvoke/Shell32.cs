using System;
using System.Runtime.InteropServices;


namespace ABC.PInvoke
{
	public static partial class Shell32
	{
		const string Dll = "shell32.dll";

		/// <summary>
		/// No official documentation available
		/// </summary>
		[DllImport( Dll, CharSet = CharSet.Auto, SetLastError = true )]
		public static extern bool RegisterShellHook( IntPtr hwnd, int flags );

		[DllImport( Dll, CharSet = CharSet.Auto, SetLastError = true )]
		public static extern uint SHAppBarMessage( int dwMessage, ref Appbardata pData );
	}
}