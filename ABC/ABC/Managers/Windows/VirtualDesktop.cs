using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ABC.Applications.Persistence;
using ABC.Common;
using Whathecode.System.Extensions;
using Whathecode.System.Windows;


namespace ABC.Managers.Windows
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
	public class VirtualDesktop : AbstractWorkspace<StoredSession>
	{
		internal delegate void UnresponsiveWindowsHandler( List<WindowSnapshot> unresponsiveWindows, VirtualDesktop desktop );

		/// <summary>
		///   Event which is triggered when unresponsive windows are detected.
		/// </summary>
		internal event UnresponsiveWindowsHandler UnresponsiveWindowDetected;

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

		List<PersistedApplication> _persistedApplications = new List<PersistedApplication>();

		internal ReadOnlyCollection<PersistedApplication> PersistedApplications
		{
			get { return _persistedApplications.AsReadOnly(); }
		}


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
		///   Unresponsive window event is triggered when newly added, hidden window is detected as unresponsive.
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
				Show();
			}
		}

		/// <summary>
		///   Remove windows which no longer belong to this desktop.
		///   Unresponsive event is triggered when any unresponsive windows are detected during remove operation.
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

			TriggerIfUnresponsive( unresponsiveWindows );
		}

		/// <summary>
		///   Show all windows associated with this virtual desktop.
		///   Unresponsive window event is triggered when any unresponsive windows are detected during show operation.
		/// </summary>
		internal void Show()
		{
			IsVisible = true;

			// Early out when virtual desktop has not windows.
			if ( _windows.Count == 0 )
			{
				return;
			}

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

			TriggerIfUnresponsive( unresponsiveWindows );
		}

		/// <summary>
		/// Hide all windows associated with this virtual desktop.
		/// Unresponsive window event is triggered when any unresponsive windows are detected during hide operation.
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

			TriggerIfUnresponsive( unresponsiveWindows );
		}

		void TriggerIfUnresponsive( List<WindowSnapshot> unresponsiveWindows )
		{
			if ( unresponsiveWindows.Count == 0 )
			{
				return;
			}

			UnresponsiveWindowDetected( unresponsiveWindows, this );
		}

		/// <summary>
		///   Find z-order of all passed windows.
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

		public override bool HasResourcesToSuspend()
		{
			return Windows.Count > 0;
		}

		/// <summary>
		///   Suspends this desktop by closing all open windows and storing their state.
		/// </summary>
		protected override void SuspendInner()
		{
			_persistedApplications = _persistenceProvider.Suspend( _windows.Select( w => new Window( w.Info ) ).ToList() );
		}

		protected override void ForceSuspendInner()
		{
			// Nothing to do.
			// The desktop manager needs to decide how to force suspension of workspaces by consuming the ForceSuspendRequested event.
		}

		/// <summary>
		///   Resumes the desktop when it was suspended by reopening all windows which are stored in StoredSession.
		/// </summary>
		protected override void ResumeInner()
		{
			_persistenceProvider.Resume( _persistedApplications );
			_persistedApplications.Clear();
		}

		/// <summary>
		///   Serialize the current desktop to a structure, allowing to restore it at a later time.
		/// </summary>
		/// <returns>A structure holding the relevant information for this desktop.</returns>
		protected override StoredSession StoreInner()
		{
			return new StoredSession( this );
		}
	}
}