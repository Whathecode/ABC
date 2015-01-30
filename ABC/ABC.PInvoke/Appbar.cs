using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using WpfWindow = System.Windows.Window;


namespace ABC.PInvoke
{
	public abstract class Appbar : WpfWindow
	{
		#region Abstract Methods

		/// <summary>
		/// Udating the DockPosition will cause the taskbar to update its position on the
		/// windows Desktop. The UpdateBorders, UpdateInterface and UpdateSize need to be handled by
		/// the main window.
		/// </summary>
		protected abstract void UpdateBorders();

		protected abstract void UpdateInterface();

		public abstract int VerticalModeSize { get; set; }
		public abstract int HorizontalModeSize { get; set; }

		#endregion


		public Orientation BarOrientation { get; private set; }

		DockPosition _dockPosition;

		public DockPosition DockPosition
		{
			get { return _dockPosition; }
			set
			{
				_dockPosition = value;

				if ( _dockPosition != DockPosition.None )
				{
	   
					BarOrientation = IsHorizontal(_dockPosition);
					if ( BarOrientation == Orientation.Horizontal )
						Height = HorizontalModeSize;
					else
						Width = VerticalModeSize;
				}
				DockingManager.DockWindow( this, _dockPosition, AutoHide );
				UpdateBorders();
				UpdateInterface();
			}
		}

		public bool AutoHide { get; set; }

		protected Appbar()
		{
			Loaded += TaskbarLoaded;

			ShowInTaskbar = false;
		}

		static Orientation IsHorizontal( DockPosition pos )
		{
			return ( pos == DockPosition.Top || pos == DockPosition.Bottom || pos == DockPosition.None )
				? Orientation.Horizontal
				: Orientation.Vertical;
		}

		void TaskbarLoaded( object sender, RoutedEventArgs e )
		{
			InitializeTaskbarStyle();
		}

		/// <summary>
		/// Glass is enabled in when the souce window is initialized
		/// </summary>
		/// <param name="e"></param>
		protected override void OnSourceInitialized( EventArgs e )
		{
			base.OnSourceInitialized( e );

			var canRenderInGlass = false;

			DwmApi.DwmIsCompositionEnabled( ref canRenderInGlass );

			if ( canRenderInGlass )
				InitializeGlass();
		}

		/// <summary>
		/// Uses DwmApi to render the entire window in Aero glass
		/// </summary>
		void InitializeGlass()
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
			DwmApi.DwmSetWindowAttribute( hwnd, DwmApi.DwmWindowAttribute.DWMWA_NCRENDERING_POLICY, ref dwmncrpDisabled, sizeof( int ) );
			Topmost = true;

			Focus();
		}

		void InitializeTaskbarStyle()
		{
			var source = HwndSource.FromHwnd( new WindowInteropHelper( this ).Handle );
			if ( source != null ) source.AddHook( WndProc );

			if ( source == null ) return;
			var exStyle = (int)Whathecode.Interop.User32.GetWindowLongPtr( source.Handle, (int)Shell32.GetWindowLongConst.GWL_EXSTYLE );

			exStyle |= (int)Whathecode.Interop.User32.ExtendedWindowStyles.ToolWindow;

			User32.SetWindowLongPtr( source.Handle, (int)Shell32.GetWindowLongConst.GWL_EXSTYLE, (uint)exStyle );

			DockPosition = DockPosition.Top;
		}

		/// <summary>
		/// Hooks into the Windows Message Processor to keep the window active
		/// </summary>
		IntPtr WndProc( IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled )
		{
			var returnvalue = IntPtr.Zero;
			if ( msg == (int)WindowMessage.WM_NCACTIVATE )
			{
				returnvalue = User32.DefWindowProc( hwnd, (int)WindowMessage.WM_NCACTIVATE, new IntPtr( 1 ), new IntPtr( -1 ) );
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