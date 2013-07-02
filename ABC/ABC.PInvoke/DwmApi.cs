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

        /// <summary>
        /// Enables the blur effect on a specified window.
        /// </summary>
        /// <param name="windowHandle">The handle to the window on which the blur behind data is applied.</param>
        /// <param name="blurBehind">A pointer to a DWM_BLURBEHIND structure that provides blur behind data.</param>
        [DllImport(Dll, PreserveSig = false)]
        public static extern void DwmEnableBlurBehindWindow(IntPtr windowHandle, ref DwmBlurbehind blurBehind);

        #endregion // Window thumbnails.

        #region Window Glass
        /// <summary>
        /// Sets the value of non-client rendering attributes for a window.
        /// </summary>
        /// <param name="windowHandle">The handle to the window that will receive the attributes.</param>
        /// <param name="atrribute">A single DWMWINDOWATTRIBUTE flag to apply to the window. This parameter specifies the attribute and the pvAttribute parameter points to the value of that attribute.</param>
        /// <param name="attributeValue">A pointer to the value of the attribute specified in the dwAttribute parameter. Different DWMWINDOWATTRIBUTE flags require different value types.</param>
        /// <param name="attributeSize">The size, in bytes, of the value type pointed to by the pvAttribute parameter.</param>
        [DllImport(Dll, PreserveSig = false)]
        public static extern void DwmSetWindowAttribute(IntPtr windowHandle, DwmWindowAttribute atrribute, ref int attributeValue, int attributeSize);

        /// <summary>
        /// Extends the window frame into the client area.
        /// </summary>
        /// <param name="windowHandle">The handle to the window in which the frame will be extended into the client area.</param>
        /// <param name="margins">A pointer to a MARGINS structure that describes the margins to use when extending the frame into the client area.</param>
        [DllImport(Dll, PreserveSig = false)]
        public static extern void DwmExtendFrameIntoClientArea(IntPtr windowHandle, ref Margins margins);

        /// <summary>
        /// Obtains a value that indicates whether Desktop Window Manager (DWM) composition is enabled. Applications can listen for composition state changes by handling the WM_DWMCOMPOSITIONCHANGED notification.
        /// </summary>
        /// <param name="enabled">A pointer to a value that, when this function returns successfully, receives TRUE if DWM composition is enabled; otherwise, FALSE.</param>
        [DllImport(Dll, PreserveSig = false)]
        public static extern void DwmIsCompositionEnabled(ref bool enabled);

        /// <summary>
        /// Enables or disables Desktop Window Manager (DWM) composition.
        /// </summary>
        /// <param name="enabled">True to enable DWM composition; false to disable composition.</param>
        [DllImport(Dll, PreserveSig = false)]
        public static extern void DwmEnableComposition(bool enabled);

        /// <summary>
        /// Retrieves the current color used for Desktop Window Manager (DWM) glass composition. This value is based on the current color scheme and can be modified by the user. Applications can listen for color changes by handling the WM_DWMCOLORIZATIONCOLORCHANGED notification.
        /// </summary>
        /// <param name="color">A pointer to a value that, when this function returns successfully, receives the current color used for glass composition. The color format of the value is 0xAARRGGBB.</param>
        /// <param name="isOpaque">A pointer to a value that, when this function returns successfully, indicates whether the color is an opaque blend. TRUE if the color is an opaque blend; otherwise, FALSE.</param>
        [DllImport(Dll, PreserveSig = false)]
        public static extern void DwmGetColorizationColor(out int color,  [MarshalAs(UnmanagedType.Bool)]out bool isOpaque);
        #endregion
    }
}