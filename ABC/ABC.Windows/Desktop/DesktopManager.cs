using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using ABC.Windows.Desktop.Server;
using ABC.Windows.Desktop.Settings;
using Whathecode.System.Extensions;


namespace ABC.Windows.Desktop
{
	/// <summary>
	///   Allows creating and switching between different <see cref="VirtualDesktop" />'s.
	/// </summary>
	/// <license>
	///   This file is part of VirtualDesktopManager.
	///   VirtualDesktopManager is free software: you can redistribute it and/or modify
	///   it under the terms of the GNU General Public License as published by
	///   the Free Software Foundation, either version 3 of the License, or
	///   (at your option) any later version.
	///
	///   VirtualDesktopManager is distributed in the hope that it will be useful,
	///   but WITHOUT ANY WARRANTY; without even the implied warranty of
	///   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	///   GNU General Public License for more details.
	///
	///   You should have received a copy of the GNU General Public License
	///   along with VirtualDesktopManager.  If not, see http://www.gnu.org/licenses/.
	/// </license>
	public class DesktopManager
	{
		internal readonly List<VirtualDesktop> AvailableDesktops = new List<VirtualDesktop>();
		internal readonly Stack<Window> WindowClipboard = new Stack<Window>();

		readonly Func<WindowInfo, bool> _windowFilter;
		readonly Func<WindowInfo, DesktopManager, List<WindowInfo>> _hideBehavior;
		readonly List<WindowInfo> _invalidWindows = new List<WindowInfo>();

		// ReSharper disable NotAccessedField.Local
		readonly MonitorVdmPipeServer _monitorServer;
		// ReSharper restore NotAccessedField.Local
		readonly WindowMonitor _windowMonitor;

		public VirtualDesktop StartupDesktop { get; private set; }
		public VirtualDesktop CurrentDesktop { get; private set; }


		/// <summary>
		///   Initializes a new desktop manager and creates one startup desktop containing all currently open windows.
		///   This desktop is accessible through the <see cref="CurrentDesktop" /> property.
		/// </summary>
		/// <param name = "settings">
		///   Contains settings for how the desktop manager should behave. E.g. which windows to ignore.
		/// </param>
		public DesktopManager( ISettings settings )
		{
			Contract.Requires( settings != null );

			if ( Environment.Is64BitOperatingSystem && !Environment.Is64BitProcess )
			{
				throw new BadImageFormatException( "A 64 bit version is needed for the Virtual Desktop Manager in order to run on a 64 bit platform." );
			}

			// Determine which windows shouldn't be managed by the desktop manager.
			_windowFilter = settings.CreateWindowFilter();
			_hideBehavior = settings.CreateHideBehavior();

			// Initialize startup desktop.
			StartupDesktop = new VirtualDesktop( GetNewWindows() )
			{
				Folder = Environment.GetFolderPath( Environment.SpecialFolder.Desktop )
			};
			CurrentDesktop = StartupDesktop;
			AvailableDesktops.Add( CurrentDesktop );			

			// Initialize window monitor.
			_windowMonitor =  new WindowMonitor();
			_windowMonitor.WindowActivated += ( window, fullscreen ) => UpdateWindowAssociations();
			_windowMonitor.WindowDestroyed += pointer => UpdateWindowAssociations();
			Task.Factory.StartNew( () => _windowMonitor.Start() );

			_monitorServer = new MonitorVdmPipeServer( this );
		}


		/// <summary>
		///   Create an empty virtual desktop with no windows assigned to it.
		/// </summary>
		/// <returns>The newly created virtual desktop.</returns>
		public VirtualDesktop CreateEmptyDesktop()
		{
			var newDesktop = new VirtualDesktop();
			AvailableDesktops.Add( newDesktop );

			return newDesktop;
		}

		/// <summary>
		///   Create an empty virtual desktop with no windows assigned to it.
		/// </summary>
		/// <returns>The newly created virtual desktop.</returns>
		public VirtualDesktop CreateEmptyDesktop( string folder )
		{
			var newDesktop = new VirtualDesktop { Folder = folder };
			AvailableDesktops.Add( newDesktop );

			return newDesktop;
		}

		/// <summary>
		///   Creates a new desktop from a stored session.
		/// </summary>
		/// <param name="session">The newly created virtual desktop.</param>
		public VirtualDesktop CreateDesktopFromSession( StoredSession session )
		{
			// The startup desktop contains all windows open at startup.
			// Windows from previously stored sessions shouldn't be assigned to this startup desktop, so remove them.
			StartupDesktop.RemoveWindows( session.OpenWindows.ToList() );

			var restored = new VirtualDesktop( session );
			AvailableDesktops.Add( restored );

			return restored;
		}

		/// <summary>
		///   Update which windows are associated to the current virtual desktop.
		/// </summary>
		public void UpdateWindowAssociations()
		{
			// Update window associations for the currently open desktop.
			CurrentDesktop.AddWindows( GetNewWindows() );
			CurrentDesktop.RemoveWindows( CurrentDesktop.Windows.Where( w => !IsValidWindow( w.Info ) ).ToList() );
			CurrentDesktop.Windows.ForEach( w => w.Update() );

			// Remove destroyed windows from places where they are cached.
			var destroyedWindows = _invalidWindows.Where( w => w.IsDestroyed() ).ToList();
			foreach ( var w in destroyedWindows )
			{
				_invalidWindows.Remove( w );
			}
		}

		/// <summary>
		///   Switch to the given virtual desktop.
		/// </summary>
		/// <param name="desktop">The desktop to switch to.</param>
		public void SwitchToDesktop( VirtualDesktop desktop )
		{
			if ( CurrentDesktop == desktop )
			{
				return;
			}

			CurrentDesktop.Icons = DesktopIconManager.SaveDestopIcons();

			// Hide windows and show those from the new desktop.
			UpdateWindowAssociations();
			CurrentDesktop.Hide( w => _hideBehavior( w, this ) );
			desktop.Show();

			// Update desktop icons.
			if ( desktop.Folder != null )
			{
				DesktopFolderSwitcher.ChangeDesktopFolder( desktop.Folder );
			}
			if ( desktop.Icons != null )
			{
				DesktopIconManager.ArrangeDesktopIcons( desktop.Icons );
			}

			CurrentDesktop = desktop;
		}

		/// <summary>
		///   Merges all windows from one desktop with those from another, and removes the original desktop.
		///   You can't merge the <see cref="StartupDesktop"/> with another desktop.
		/// </summary>
		/// <returns>A new virtual desktop which has windows of both passed desktops assigned to it.</returns>
		public void Merge( VirtualDesktop from, VirtualDesktop to )
		{
			Contract.Requires( from != null && to != null && from != to );

			if ( from == StartupDesktop )
			{
				throw new ArgumentException( "Can't remove the startup desktop.", "from" );
			}
			if ( from == CurrentDesktop )
			{
				throw new ArgumentException( "The passed desktop can't be removed since it's the current desktop.", "from" );
			}

			AvailableDesktops.Remove( from );
			to.AddWindows( from.Windows.ToList() );
		}

		/// <summary>
		///   Closes the virtual desktop manager by restoring all windows.
		/// </summary>
		public void Close()
		{
			AvailableDesktops.ForEach( d => d.Show() );

			// Show all cut windows again.
			var showWindows = WindowClipboard.Select( w => new RepositionWindowInfo( w.Info ) { Visible = w.Visible } );
			WindowManager.RepositionWindows( showWindows.ToList(), true );
		}

		/// <summary>
		///   Check whether the WindowInfo object represents a valid Window.
		/// </summary>
		/// <returns>True if valid, false if unvalid.</returns>
		bool IsValidWindow( WindowInfo window )
		{
			return !window.IsDestroyed() && _windowFilter( window );
		}

		/// <summary>
		///   Gets all newly created windows.
		/// </summary>
		/// <returns>A list with all new windows.</returns>
		List<Window> GetNewWindows()
		{
			List<WindowInfo> newWindows = WindowManager.GetWindows().Except(
				AvailableDesktops.SelectMany( d => d.Windows ).Concat( WindowClipboard )
				.Select( w => w.Info )
				.Concat( _invalidWindows ) ).ToList();

			var validWindows = new List<Window>();
			foreach ( var w in newWindows )
			{
				if ( IsValidWindow( w ) )
				{
					validWindows.Add( new Window( w ) );
				}
				else
				{
					_invalidWindows.Add( w );
				}
			}
			return validWindows;
		}

		/// <summary>
		///   Cut a given window from the currently open desktop and store it in a clipboard.
		///   TODO: What if a window from a different desktop is passed? Should this be supported?
		/// </summary>
		public void CutWindow( WindowInfo window )
		{
			UpdateWindowAssociations(); // Newly added windows might need to be cut too, so update associations first.

			if ( !_windowFilter( window ) )
			{
				return;
			}

			var cutWindows = _hideBehavior( window, this ).Select( w => new Window( w ) ).ToList();
			cutWindows.ForEach( w => WindowClipboard.Push( w ) );
			CurrentDesktop.RemoveWindows( cutWindows );
		}

		/// <summary>
		///   Paste all windows on the clipboard on the currently open desktop.
		/// </summary>
		public void PasteWindows()
		{
			UpdateWindowAssociations();	// There might be newly added windows of which the z-order isn't known yet, so update associations first.

			CurrentDesktop.AddWindows( WindowClipboard.ToList() );
			WindowClipboard.Clear();
		}
	}
}