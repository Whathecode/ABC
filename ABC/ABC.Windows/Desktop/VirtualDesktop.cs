using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ABC.Applications.Persistence;
using ABC.Common;
using Whathecode.System.Extensions;
using Whathecode.System.Windows.Interop;


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
		readonly PersistenceProvider _persistenceProvider;

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

		List<PersistedApplication>  _persistedApplications = new List<PersistedApplication>();
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
		internal VirtualDesktop( PersistenceProvider persistenceProvider )
		{
			_persistenceProvider = persistenceProvider;
		}

		/// <summary>
		///   Create a virtual desktop from a previously stored session.
		/// </summary>
		/// <param name = "session">The previously stored session.</param>
		/// <param name = "persistenceProvider">Provider which allows to persist application state.</param>
		internal VirtualDesktop( StoredSession session, PersistenceProvider persistenceProvider )
			: this( persistenceProvider )
		{
			_persistedApplications = session.PersistedApplications.ToList();
			IsSuspended = _persistedApplications.Count > 0;
			_windows.AddRange( session.OpenWindows );
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
			_windows = toAdd.Concat( OrderWindowsByZOrder( _windows ) ).ToList();

			// Make sure to show the newly added windows in case they were hidden.
			if ( IsVisible && toAdd.Any( w => w.Visible && !w.Info.IsVisible() ) )
			{
				Show();
			}
		}

		/// <summary>
		///   Remove windows which no longer belong to this desktop.
		/// </summary>
		/// <param name = "toRemove">Windows which no longer belong to the desktop.</param>
		internal void RemoveWindows( List<WindowSnapshot> toRemove )
		{
			toRemove.ForEach( w => _windows.Remove( w ) );

			if ( IsVisible && toRemove.Any() )
			{
				// Hide windows.
				var hideWindows = toRemove
					.Where( w => !w.Info.IsDestroyed() )
					.Select( w => new RepositionWindowInfo( w.Info ) { Visible = false } );
				WindowManager.RepositionWindows( hideWindows.ToList() );
			}
		}

		/// <summary>
		///   Show all windows associated with this virtual desktop.
		/// </summary>
		internal void Show()
		{
			IsVisible = true;

			// Reposition windows.
			// Topmost windows are repositioned separately in order to prevent non-topmost windows from becoming topmost when moving them above topmost windows in the z-order.						
			var allWindows = _windows.GroupBy( w => w.Info.IsTopmost() );
			allWindows.ForEach( group =>
			{
				var showWindows = group.Select( w => new RepositionWindowInfo( w.Info ) { Visible = w.Visible } );
				WindowManager.RepositionWindows( showWindows.ToList(), true );
			} );

			// Activate top window.
			// TODO: Is the topmost window always the previous active one? Possibly a better check is needed.
			// TODO: Which window to activate when desktops are merged?
			WindowSnapshot first = _windows.FirstOrDefault( w => w.Visible );
			if ( first != null )
			{
				first.Info.SetForegroundWindow();
			}
		}

		/// <summary>
		///   Hide all windows associated with this virtual desktop.
		/// </summary>
		internal void Hide( Func<WindowInfo, List<WindowInfo>> hideBehavior )
		{
			IsVisible = false;

			_windows = OrderWindowsByZOrder( _windows );

			// Find windows to hide using process specific hide behaviors.
			var toHide = _windows.Where( w => w.Visible ).SelectMany( w => hideBehavior( w.Info ) );

			// Hide windows.
			var hideWindows = toHide.Select( w => new RepositionWindowInfo( w ) { Visible = false } );
			WindowManager.RepositionWindows( hideWindows.ToList() );
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

			IsSuspended = false;
		}
	}
}