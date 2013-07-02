using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using ABC.PInvoke;


namespace ABC.Windows
{
	public class Taskbar : Window
	{
		DockingManager.DockPosition _dockPosition;

		public DockingManager.DockPosition DockPosition
		{
			get { return _dockPosition; }
			set
			{
				_dockPosition = value;
				DockingManager.DockWindow( this, _dockPosition, AutoHide );
			}
		}

		public bool AutoHide { get; set; }


		public Taskbar()
		{
			Loaded += Taskbar_Loaded;
		}


		#region Overrides

		protected override void OnSourceInitialized( EventArgs e )
		{
			base.OnSourceInitialized( e );

			InitialGlass();
		}

		void InitialGlass()
		{
			var bb = new DwmApi.DwmBlurbehind
			{
				dwFlags = (int)DwmApi.DwmBlurBehindDwFlags.DwmBbEnable,
				fEnable = true
			};

			Background = Brushes.Transparent;
			ResizeMode = ResizeMode.NoResize;
			WindowStyle = WindowStyle.None;

			var hwnd = new WindowInteropHelper( this ).Handle;

			var hwndSource = HwndSource.FromHwnd( hwnd );
			if ( hwndSource != null )
				if ( hwndSource.CompositionTarget != null )
					hwndSource.CompositionTarget.BackgroundColor = Colors.Transparent;

			DwmApi.DwmEnableBlurBehindWindow( hwnd, ref bb );

			var dwmncrpDisabled = 2;
			DwmApi.DwmSetWindowAttribute( hwnd, DwmApi.DwmWindowAttribute.DWMWA_NCRENDERING_POLICY, ref dwmncrpDisabled,
			                              sizeof( int ) );
			Topmost = true;

			Focus();
		}

		#endregion


		void Taskbar_Loaded( object sender, RoutedEventArgs e )
		{
			var source = HwndSource.FromHwnd( new WindowInteropHelper( this ).Handle );
			if ( source != null ) source.AddHook( WndProc );

			if ( source == null ) return;
			var exStyle = (int)User32.GetWindowLongPtr( source.Handle, (int)Shell32.GetWindowLongConst.GWL_EXSTYLE );

			exStyle |= (int)User32.ExtendedWindowStyles.ToolWindow;

			User32.SetWindowLongPtr( source.Handle, (int)Shell32.GetWindowLongConst.GWL_EXSTYLE, (uint)exStyle );
		}

		IntPtr WndProc( IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled )
		{
			var returnvalue = IntPtr.Zero;
			if ( msg == (int)WindowMessage.WM_NCACTIVATE )
			{
				returnvalue = User32.DefWindowProc( hwnd, (int)WindowMessage.WM_NCACTIVATE, new IntPtr( 1 ),
				                                    new IntPtr( -1 ) );
				handled = true;
			}
			if ( msg == (int)WindowMessage.WM_ACTIVATE )
			{
				returnvalue = User32.DefWindowProc( hwnd, (int)WindowMessage.WM_ACTIVATE, new IntPtr( 1 ), new IntPtr( -1 ) );
				handled = true;
			}
			return returnvalue;
		}
	}
}