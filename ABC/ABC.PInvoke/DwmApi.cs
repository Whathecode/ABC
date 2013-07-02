using System;
using System.Runtime.InteropServices;


namespace ABC.PInvoke
{
	/// <summary>
	///   Class through which Desktop Window Manager calls (dwmapi.dll) can be accessed for which the .NET framework offers no alternative.
	/// </summary>
	public static partial class DwmApi
	{
		const string Dll = "dwmapi.dll";


		#region Window thumbnails.

		/// <summary>
		///   Creates a Desktop Window Manager (DWM) thumbnail relationship between the destination and source windows.
		/// </summary>
		/// <param name = "destinationWindowHandle">The handle to the window that will use the DWM thumbnail.</param>
		/// <param name = "sourceWindowHandle">The handle to the window to use as the thumbnail source.</param>
		/// <param name = "thumbnailHandle">A handle that, when this function returns successfully, represents the registration of the DWM thumbnail.</param>
		/// <exception cref = "ArgumentException">The destination and source window handles should be top-level windows.</exception>
		/// <remarks>
		///   Registering a DWM thumbnail relationship will not modify desktop composition; for information about thumbnail positioning, see the documentation for the <see cref="DwmUpdateThumbnailProperties" /> function.
		///   The window designated by <see cref="destinationWindowHandle"/> must either be the desktop window itself or be owned by the process that is calling <see cref="DwmRegisterThumbnail"/>
		///   This is required to prevent applications from affecting the content of other applications.
		///   The thumbnail registration handle obtained by this function is not globally unique but is unique to the process.
		///   Call the <see cref="DwmUnregisterThumbnail" /> function to unregister the thumbnail. This must be done within the process that the relationship was registered in.
		/// </remarks>
		[DllImport( Dll, PreserveSig = false )]
		public static extern void DwmRegisterThumbnail( IntPtr destinationWindowHandle, IntPtr sourceWindowHandle, out IntPtr thumbnailHandle );

		/// <summary>
		///   Removes a Desktop Window Manager (DWM) thumbnail relationship created by the <see cref="DwmRegisterThumbnail" /> function.
		/// </summary>
		/// <param name = "thumbnailHandle">The handle to the thumbnail relationship to be removed.</param>
		/// <exception cref = "ArgumentException">Non-existent thumbanil handle is passed.</exception>
		[DllImport( Dll, PreserveSig = false )]
		public static extern void DwmUnregisterThumbnail( IntPtr thumbnailHandle );

		/// <summary>
		///   Updates the properties for a Desktop Window Manager (DWM) thumbnail.
		/// </summary>
		/// <param name = "thumbnailHandle">The handle to the DWM thumbnail to be updated.</param>
		/// <param name = "properties">A pointer to a <see cref="ThumbnailProperties" /> structure that contains the new thumbnail properties.</param>
		/// <exception cref = "ArgumentException">Non-existent thumbanil handle or handle owned by other process is passed.</exception>
		[DllImport( Dll, PreserveSig = false )]
		public static extern void DwmUpdateThumbnailProperties( IntPtr thumbnailHandle, ref ThumbnailProperties properties );

		#endregion // Window thumbnails.
	}
}