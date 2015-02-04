using System;
using System.Windows;
using WUser32 = Whathecode.Interop.User32;


namespace ABC.PInvoke
{
	/// <summary>
	///   Experimental thubmnail class.
	///   TODO: Steven Houben, what is this? I moved it outside of the WindowInfo class since I'm using the central FCL library again to avoid code duplication.
	/// </summary>
	class ThumbnailInfo
	{
		readonly IntPtr _windowHandle;
		IntPtr _thumbnailHandle;


		public ThumbnailInfo( IntPtr windowHandle )
		{
			_windowHandle = windowHandle;
		}


		public void RegisterThumbnail( IntPtr baseWindow, ref System.Windows.Controls.Image image, Point location, Size size )
		{
			if ( _thumbnailHandle != IntPtr.Zero )
			{
				DwmApi.DwmUnregisterThumbnail( _thumbnailHandle );
			}

			DwmApi.DwmRegisterThumbnail( baseWindow, _windowHandle, out _thumbnailHandle );

			if ( _thumbnailHandle == IntPtr.Zero )
			{
				return;
			}

			var thumbnailProperties = new DwmApi.ThumbnailProperties
			{
				dwFlags = DwmApi.ThumbnailProperties.DWM_TNP_VISIBLE +
				          DwmApi.ThumbnailProperties.DWM_TNP_OPACITY +
				          DwmApi.ThumbnailProperties.DWM_TNP_RECTDESTINATION,
				opacity = 255,
				fVisible = true,
				rcDestination = new WUser32.Rectangle
				{
					Left = (int)location.X,
					Top = (int)location.Y,
					Right = (int)size.Width,
					Bottom = (int)size.Height
				}
			};

			DwmApi.DwmUpdateThumbnailProperties( _thumbnailHandle, ref thumbnailProperties );
		}

		public void UnregisterThumbnail()
		{
			if ( _thumbnailHandle != IntPtr.Zero )
				DwmApi.DwmUnregisterThumbnail( _thumbnailHandle );
		}
	}
}
