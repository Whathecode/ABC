using System;
using System.Runtime.InteropServices;
using WUser32 = Whathecode.Interop.User32;


namespace ABC.PInvoke
{
	public partial class DwmApi
	{
		[StructLayout( LayoutKind.Sequential )]
		public struct ThumbnailProperties
		{
			public uint dwFlags;
			public WUser32.Rectangle rcDestination;
			public WUser32.Rectangle rcSource;
			public byte opacity;

			[MarshalAs( UnmanagedType.Bool )]
			public bool fVisible;

			[MarshalAs( UnmanagedType.Bool )]
			public bool fSourceClientAreaOnly;

			public const uint DWM_TNP_RECTDESTINATION = 0x00000001;
			public const uint DWM_TNP_RECTSOURCE = 0x00000002;
			public const uint DWM_TNP_OPACITY = 0x00000004;
			public const uint DWM_TNP_VISIBLE = 0x00000008;
			public const uint DWM_TNP_SOURCECLIENTAREAONLY = 0x00000010;
		}

		[Flags]
		public enum DwmBlurBehindOptions
		{
			DWM_BB_ENABLE = 0x00000001,
			DWM_BB_BLURREGION = 0x00000002,
			DWM_BB_TRANSITIONONMAXIMIZED = 0x00000004
		}

		[Flags]
		public enum DwmWindowAttribute
		{
			DWMWA_NCRENDERING_ENABLED = 1,
			DWMWA_NCRENDERING_POLICY,
			DWMWA_TRANSITIONS_FORCEDISABLED,
			DWMWA_ALLOW_NCPAINT,
			DWMWA_CAPTION_BUTTON_BOUNDS,
			DWMWA_NONCLIENT_RTL_LAYOUT,
			DWMWA_FORCE_ICONIC_REPRESENTATION,
			DWMWA_FLIP3D_POLICY,
			DWMWA_EXTENDED_FRAME_BOUNDS,
			DWMWA_HAS_ICONIC_BITMAP,
			DWMWA_DISALLOW_PEEK,
			DWMWA_EXCLUDED_FROM_PEEK,
			DWMWA_LAST
		}

		[Flags]
		public enum DwmncRenderingPolicy : uint
		{
			UseWindowStyle,
			Disabled,
			Enabled,
			Last
		}

		[Flags]
		public enum DwmBlurBehindDwFlags
		{
			DwmBbEnable = 1,
			DwmBbBlurRegion = 2,
			DwmBbTransitionOnMaximized = 4
		}

		[StructLayout( LayoutKind.Sequential )]
		public struct DwmBlurbehind
		{
			public int dwFlags;
			public bool fEnable;
			public IntPtr hRgnBlur;
			public bool fTransitionOnMaximized;
		}

		[StructLayout( LayoutKind.Sequential )]
		public struct Margins
		{
			public int cxLeftWidth;
			public int cxRightWidth;
			public int cyTopHeight;
			public int cyBottomHeight;
		}
	}
}