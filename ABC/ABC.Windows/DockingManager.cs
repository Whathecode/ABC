using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using ABC.PInvoke;
using Point = System.Windows.Point;
using Size = System.Windows.Size;

namespace ABC.Windows
{
	public class DockingManager
	{
		static readonly Dictionary<Window, RegisterInfo> RegisteredWindowInfo = new Dictionary<Window, RegisterInfo>();

		public enum DockPosition
		{
			Left,
			Top,
			Right,
			Bottom,
			None
		}
		static RegisterInfo GetRegisterInfo( Window appbarWindow )
		{
			RegisterInfo reg;
			if ( RegisteredWindowInfo.ContainsKey( appbarWindow ) )
			{
				reg = RegisteredWindowInfo[ appbarWindow ];
			}
			else
			{
				reg = new RegisterInfo
				{
					CallbackId = 0,
					Window = appbarWindow,
					IsRegistered = false,
					Edge = DockPosition.Top,
					OriginalStyle = appbarWindow.WindowStyle,
					OriginalPosition = new Point( appbarWindow.Left, appbarWindow.Top ),
					OriginalSize = new Size( appbarWindow.ActualWidth, appbarWindow.ActualHeight ),
					OriginalResizeMode = appbarWindow.ResizeMode,
				};
				RegisteredWindowInfo.Add( appbarWindow, reg );
			}
			return reg;
		}

		static void RestoreWindow( Window appbarWindow )
		{
			RegisterInfo info = GetRegisterInfo( appbarWindow );

			appbarWindow.WindowStyle = info.OriginalStyle;
			appbarWindow.ResizeMode = info.OriginalResizeMode;

			var rect = new Rect( info.OriginalPosition.X, info.OriginalPosition.Y,
			                     info.OriginalSize.Width, info.OriginalSize.Height );
			appbarWindow.Dispatcher.BeginInvoke( DispatcherPriority.ApplicationIdle,
			                                     new ResizeDelegate( DoResize ), appbarWindow, rect );
		}

		delegate void ResizeDelegate( Window appbarWindow, Rect rect );

		static void DoResize( Window appbarWindow, Rect rect )
		{
			appbarWindow.Width = rect.Width;
			appbarWindow.Height = rect.Height;
			appbarWindow.Top = rect.Top;
			appbarWindow.Left = rect.Left;
		}

		static void SetPostion( DockPosition edge, Window appbarWindow, bool isAutoHide )
		{
			var barData = new Shell32.Appbardata();
			barData.cbSize = Marshal.SizeOf( barData );
			barData.hWnd = new WindowInteropHelper( appbarWindow ).Handle;
			barData.uEdge = (int)edge;

			if ( barData.uEdge == (int)DockPosition.Left || barData.uEdge == (int)DockPosition.Right )
			{
				barData.rc.Top = 0;
				barData.rc.Bottom = (int)SystemParameters.PrimaryScreenHeight;
				if ( barData.uEdge == (int)DockPosition.Left )
				{
					barData.rc.Left = 0;
					barData.rc.Right = (int)Math.Round( appbarWindow.ActualWidth );
				}
				else
				{
					barData.rc.Right = (int)SystemParameters.PrimaryScreenWidth;
					barData.rc.Left = barData.rc.Right - (int)Math.Round( appbarWindow.ActualWidth );
				}
			}
			else
			{
				barData.rc.Left = 0;
				barData.rc.Right = (int)SystemParameters.PrimaryScreenWidth;
				if ( barData.uEdge == (int)DockPosition.Top )
				{
					barData.rc.Top = 0;
					barData.rc.Bottom = (int)Math.Round( appbarWindow.ActualHeight );
				}
				else
				{
					barData.rc.Bottom = (int)SystemParameters.PrimaryScreenHeight;
					barData.rc.Top = barData.rc.Bottom - (int)Math.Round( appbarWindow.ActualHeight );
				}
			}

			Shell32.SHAppBarMessage( (int)Shell32.AppBarMessages.ABM_QUERYPOS, ref barData );
            if (isAutoHide)
                Shell32.SHAppBarMessage((int)Shell32.AppBarMessages.ABM_SETAUTOHIDEBAR, ref barData);    
            else
			    Shell32.SHAppBarMessage( (int)Shell32.AppBarMessages.ABM_SETPOS, ref barData );

			var rect = new Rect( barData.rc.Left, barData.rc.Top,
			                     barData.rc.Right - barData.rc.Left, barData.rc.Bottom - barData.rc.Top );

			appbarWindow.Dispatcher.BeginInvoke( DispatcherPriority.ApplicationIdle,
			                                     new ResizeDelegate( DoResize ), appbarWindow, rect );
		}

		public static void DockWindow( Window appbarWindow, DockPosition edge, bool autoHide )
		{
			var info = GetRegisterInfo( appbarWindow );
			info.Edge = edge;
		    info.AutoHide = autoHide;

			var abd = new Shell32.Appbardata();
			abd.cbSize = Marshal.SizeOf( abd );
			abd.hWnd = new WindowInteropHelper( appbarWindow ).Handle;

			if ( edge == DockPosition.None )
			{
				if ( info.IsRegistered )
				{
					Shell32.SHAppBarMessage( (int)Shell32.AppBarMessages.ABM_REMOVE, ref abd );
					info.IsRegistered = false;
				}
				RestoreWindow( appbarWindow );
				return;
			}

			if ( !info.IsRegistered )
			{
				info.IsRegistered = true;
				info.CallbackId = User32.RegisterWindowMessage( "AppBarMessage" );
				abd.uCallbackMessage = info.CallbackId;

				Shell32.SHAppBarMessage( (int)Shell32.AppBarMessages.ABM_NEW, ref abd );


				Shell32.SHAppBarMessage( (int)Shell32.AppBarMessages.ABM_ACTIVATE, ref abd );

				var source = HwndSource.FromHwnd( abd.hWnd );
				if ( source != null ) source.AddHook( info.WndProc );
			}

			appbarWindow.WindowStyle = WindowStyle.None;
			appbarWindow.ResizeMode = ResizeMode.NoResize;
			appbarWindow.Topmost = true;

			SetPostion( info.Edge, appbarWindow, autoHide );
		}

        internal class RegisterInfo
        {
            public int CallbackId { get; set; }
            public bool IsRegistered { get; set; }
            public Window Window { private get; set; }
            public DockPosition Edge { get; set; }
            public WindowStyle OriginalStyle { get; set; }
            public Point OriginalPosition { get; set; }
            public Size OriginalSize { get; set; }
            public ResizeMode OriginalResizeMode { get; set; }
            public bool AutoHide { get; set; }

            public IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
            {
                if (msg == CallbackId)
                {
                    if (wParam.ToInt32() == (int)Shell32.AppBarNotifications.ABN_POSCHANGED)
                    {
                        SetPostion(Edge, Window, AutoHide);
                        handled = true;
                    }
                }
                return IntPtr.Zero;
            }
        }
	}
}