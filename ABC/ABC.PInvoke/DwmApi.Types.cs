﻿using System;
using System.Runtime.InteropServices;

namespace ABC.PInvoke
{
    public partial class DwmApi
    {

        [StructLayout(LayoutKind.Sequential)]
        public struct Psize
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class DwmThumbnailProperties
        {
            public uint dwFlags;
            public RECT rcDestination;
            public RECT rcSource;
            public byte opacity;
            [MarshalAs(UnmanagedType.Bool)]
            public bool fVisible;
            [MarshalAs(UnmanagedType.Bool)]
            public bool fSourceClientAreaOnly;
            // ReSharper disable InconsistentNaming
            public const uint DWM_TNP_RECTDESTINATION = 0x00000001;
            public const uint DWM_TNP_RECTSOURCE = 0x00000002;
            public const uint DWM_TNP_OPACITY = 0x00000004;
            public const uint DWM_TNP_VISIBLE = 0x00000008;
            public const uint DWM_TNP_SOURCECLIENTAREAONLY = 0x00000010;
            // ReSharper restore InconsistentNaming
        }

        [StructLayout(LayoutKind.Sequential)]
        public class Margins
        {
            public int cxLeftWidth, cxRightWidth,
                       cyTopHeight, cyBottomHeight;

            public Margins(int left, int top, int right, int bottom)
            {
                cxLeftWidth = left; cyTopHeight = top;
                cxRightWidth = right; cyBottomHeight = bottom;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DwmBlurbehind
        {
            public CoreNativeMethods.DwmBlurBehindDwFlags dwFlags;
            public bool Enabled;
            public IntPtr BlurRegion;
            public bool TransitionOnMaximized;
        }

        public static class CoreNativeMethods
        {
            public enum DwmBlurBehindDwFlags
            {
                DwmBbEnable = 1,
                DwmBbBlurRegion = 2,
                DwmBbTransitionOnMaximized = 4
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left, top, right, bottom;

            public RECT(int left, int top, int right, int bottom)
            {
                this.left = left; this.top = top;
                this.right = right; this.bottom = bottom;
            }
        }
    }
}
