using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ABC.Applications.Persistence;
using ABC.Common;
using Whathecode.System.Extensions;
using Whathecode.System.Windows;


namespace ABC.Windows.Desktop
{
	/// <summary>
	///   Represents a virtual desktop.
	/// </summary>
	/// <author>Steven Jeuris</author>
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
	public class VirtualDesktop
	{
		List<WindowSnapshot> _windows = new List<WindowSnapshot>();
		readonly AbstractPersistenceProvider _persistenceProvider;

		public ReadOnlyCollection<Window> Windows
		{
			get { return _windows.Select( w => new Window( w.Info ) ).ToList().AsReadOnly(); }
		}

		internal ReadOnlyCollection<WindowSnapshot> WindowSnapshots
		{
			get { return _windows.AsReadOnly(); }
		}

		public bool IsVisible { get; private set; }

		/// <summary>
		///   Determines whether the desktop is currently in a suspended state,
		///   meaning all its containing processes and windows are closed and the desktop needs to be resumed prior to being able to continue work on it.
		/// </summary>
		public bool IsSuspended { get; private set; }

		List<PersistedApplication> _persistedApplications = new List<PersistedApplication>();

		internal ReadOnlyCollection<PersistedApplication> PersistedApplications
		{
			get { return _persistedApplications.AsReadOnly(); }
		}

		/// <summary>
		///   The folder associated with this desktop, which is used to populate the desktop icons.
		/// </summary>
		public string Folder { get; set; }

		public List<DesktopIcon> Icons { get; set; }


		/// <summary>
		///   Create an empty virtual desktop.
		/// </summary>
		internal VirtualDesktop( AbstractPersistenceProvider persistenceProvider )
		{
			_persistenceProvider = persistenceProvider;
		}

		/// <summary>
		///   Create a virtual desktop from a previously stored session.
		/// </summary>
		/// <param name = "session">The previously stored session.</param>
		/// <param name = "persistenceProvider">Provider which allows to persist application state.</param>
		internal VirtualDesktop( StoredSession session, AbstractPersistenceProvider persistenceProvider )
			: this( persistenceProvider )
		{
			_persistedApplications = session.PersistedApplications.ToList();
			IsSuspended = _persistedApplications.Count > 0;
			_windows.AddRange( session.OpenWindows.Where( w => !w.Info.IsDestroyed() ) );
		}


		/// <summary>
		///   Reposition a set of windows.
		/// </summary>
		/// <param name="toPosition">The windows which are being repositioned.</param>
		/// <param name="changeZOrder">
		///   When true, the windows's Z orders are changed to reflect the order of the toPosition list.
		///   The first item in the list will appear at the top, while the last item will appear at the bottom.
		/// </param>
		/// <returns>A list of unresponsive windows which could not be repositioned.</returns>
		List<WindowSnapshot> RepositionWindows( IEnumerable<RepositionWindowInfo> toPosition, bool changeZOrder )
		{
			var unresponsive = new List<WindowSnapshot>();

			try
			{
				WindowManager.RepositionWindows( toPosition.ToList(), true, changeZOrder );
			}
			catch ( Whathecode.System.Windows.UnresponsiveWindowsException e )
			{
				unresponsive = e.UnresponsiveWindows.Select( u => _windows.First( w => w.Info.Equals( u ) ) ).ToList();
			}

			return unresponsive;
		}

		/// <summary>
		///   Adds the passed new windows and shows them in case the desktop is visible.
		/// </summary>
		/// <param name = "newWindows">New windows associated to this virtual desktop.</param>
		internal void AddWindows( List<WindowSnapshot> newWindows )
		{
			// Add added windows to the front of the list so they show up in front.
			var toAdd = newWindows.Where( w => !_windows.Contains( w ) ).ToList();
			toAdd = OrderWindowsByZOrder( toAdd );
			toAdd.ForEach( w => w.ChangeDesktop( this ) );
			_windows = toAdd.Concat( OrderWindowsByZOrder( _windows ) ).ToList();

			// Make sure to show the newly added windows in case they were hidden.
			if ( IsVisible && toAdd.Any( w => w.Visible && !w.Info.IsVisible() ) )
			{
				// TODO: An UnresponsiveWindowsException can be thrown here which is not handled!
				Show();
			}
		}

		/// <summary>
		///   Remove windows which no longer belong to this desktop.
		/// </summary>
		/// <param name = "toRemove">Windows which no longer belong to the desktop.</param>
		internal void RemoveWindows( List<WindowSnapshot> toRemove )
		{
			var unresponsiveWindows = new List<WindowSnapshot>();

			if ( IsVisible && toRemove.Any() )
			{
				// Hide windows.
				IEnumerable<RepositionWindowInfo> hideWindows = toRemove
					.Where( w => !w.Ignore && !w.Info.IsDestroyed() )
					.Select( w => new RepositionWindowInfo( w.Info ) { Visible = false } );
				unresponsiveWindows = RepositionWindows( hideWindows, false );
			}

			toRemove.ForEach( w =>
			{
				w.ChangeDesktop( null );
				_windows.Remove( w );
			} );

			ThrowIfUnresponsive( unresponsiveWindows );
		}

		/// <summary>
		///   Show all windows associated with this virtual desktop.
		/// </summary>
		internal void Show()
		{
			IsVisible = true;

			var unresponsiveWindows = new List<WindowSnapshot>();

			// Reposition windows.
			// Topmost windows are repositioned separately in order to prevent non-topmost windows from becoming topmost when moving them above topmost windows in the z-order.
			var allWindows = _windows
				.Where( w => !w.Ignore )
				.GroupBy( w => w.Info.IsTopmost() );
			allWindows.ForEach( group =>
			{
				var showWindows = group.Select( w => new RepositionWindowInfo( w.Info ) { Visible = w.Visible } );
				unresponsiveWindows.AddRange( RepositionWindows( showWindows, true ) );
			} );

			// Activate top window.
			// TODO: Is the topmost window always the previous active one? Possibly a better check is needed.
			// TODO: Which window to activate when desktops are merged?
			WindowSnapshot first = _windows.FirstOrDefault( w => w.Visible && !unresponsiveWindows.Contains( w ) );
			if ( first != null )
			{
				first.Info.SetForegroundWindow();
			}

			ThrowIfUnresponsive( unresponsiveWindows );
		}

		/// <summary>
		///   Hide all windows associated with this virtual desktop.
		/// </summary>
		internal void Hide( Func<WindowInfo, List<WindowInfo>> hideBehavior )
		{
			IsVisible = false;

			_windows = OrderWindowsByZOrder( _windows );

			// Find windows to hide using process specific hide behaviors.
			var toHide = _windows
				.Where( w => !w.Ignore && w.Visible )
				.SelectMany( w => hideBehavior( w.Info ) )
				.ToList();

			// Hide windows.
			var hideWindows = toHide.Select( w => new RepositionWindowInfo( w ) { Visible = false } );
			List<WindowSnapshot> unresponsiveWindows = RepositionWindows( hideWindows, false );

			ThrowIfUnresponsive( unresponsiveWindows );
		}

		void ThrowIfUnresponsive( List<WindowSnapshot> unresponsiveWindows )
		{
			if ( unresponsiveWindows.Count == 0 )
			{
				return;
			}

			throw new UnresponsiveWindowsException( this, unresponsiveWindows );
		}

		/// <summary>
		///    Find z-order of all passed windows.
		/// </summary>
		/// <returns>A new list containing all the passed windows, but ordered according to their z-order, most on top first.</returns>
		static List<WindowSnapshot> OrderWindowsByZOrder( List<WindowSnapshot> toOrder )
		{
			// TODO: Safeguard for infinite loop and possible destroyed windows.
			// http://stackoverflow.com/q/12992201/590790
			var ordenedWindows = new List<WindowSnapshot>();
			WindowInfo window = WindowManager.GetTopWindow();
			while ( window != null )
			{
				WindowSnapshot match = toOrder.FirstOrDefault( w => w.Info.Equals( window ) );
				if ( match != null )
				{
					ordenedWindows.Add( match );
				}
				window = WindowManager.GetWindowBelow( window );
			}

			return ordenedWindows;
		}

		/// <summary>
		///   Serialize the current desktop to a structure, allowing to restore it at a later time.
		/// </summary>
		/// <returns>A structure holding the relevant information for this desktop.</returns>
		public StoredSession Store()
		{
			return new StoredSession( this );
		}

		/// <summary>
		///   Suspends this desktop by closing all open windows and storing their state in StoredSession.
		/// </summary>
		public void Suspend()
		{
			if ( IsSuspended )
			{
				return;
			}

			_persistedApplications = _persistenceProvider.Suspend( _windows.Select( w => new Window( w.Info ) ).Cast<IWindow>().ToList() );

			IsSuspended = true;
		}

		/// <summary>
		///   Resumes the desktop when it was suspended by reopening all windows which are stored in StoredSession.
		/// </summary>
		public void Resume()
		{
			if ( !IsSuspended )
			{
				return;
			}

			_persistenceProvider.Resume( _persistedApplications );
			_persistedApplications.Clear();

			IsSuspended = false;
		}

		/// <summary>
		///   Transfer windows which are part of the currently visible desktop to another desktop.
		/// </summary>
		/// <param name = "toTransfer">The windows to transfer.</param>
		/// <param name = "destination">The virtual desktop to which the windows will be transferred.</param>
		public void TransferWindows( List<Window> toTransfer, VirtualDesktop destination )
		{
			// TODO: Is there any way to always support this? The internal WindowSnapshot constructor assumes the windows are part of the currently visible desktop.
			if ( !IsVisible )
			{
				throw new InvalidOperationException( "Can only transfer windows when the desktop is currently visible." );
			}

			List<WindowSnapshot> snapshots = toTransfer.Select( w => new WindowSnapshot( this, w.WindowInfo ) ).ToList();
			// TODO: An UnresponsiveWindowsException can be thrown here which is not handled!
			RemoveWindows( snapshots );
			destination.AddWindows( snapshots );
		}
	}
}