using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

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
		List<Window> _windows = new List<Window>();
		public ReadOnlyCollection<Window> Windows
		{
			get { return _windows.AsReadOnly(); }
		}

        public string Folder { get; set; }

        public List<DesktopIcon> Icons { get; set; }

	    /// <summary>
		///   Create an empty virtual desktop.
		/// </summary>
		internal VirtualDesktop() {}

		/// <summary>
		///   Create a virtual desktop which is initialized with a set of existing windows.
		/// </summary>
		/// <param name = "initialWindows">The windows which should belong to the new virtual desktop.</param>
		internal VirtualDesktop( IEnumerable<Window> initialWindows )
		{
			_windows.AddRange( initialWindows );
		}

		/// <summary>
		///   Create a virtual desktop from a previously stored session.
		/// </summary>
		/// <param name = "session">The previously stored session.</param>
		internal VirtualDesktop( StoredSession session )
		{
			_windows.AddRange( session.OpenWindows );
		}

		/// <summary>
		///   Adds the passed new windows and removes windows which are no longer open from the list.
		/// </summary>
		/// <param name = "newWindows">Newly opened windows on this virtual desktop.</param>
		/// <param name = "toRemove">Windows which no longer belong to the desktop.</param>
		internal void UpdateWindowAssociations( List<Window> newWindows, List<Window> toRemove )
		{
			toRemove.ForEach( w => _windows.Remove( w ) );
			_windows.AddRange( newWindows );
			_windows.ForEach( w => w.Update() );
		}

		/// <summary>
		///   Adds the passed window to the virtual desktop and activates it.
		/// </summary>
		/// <param name = "toAdd">The window to add.</param>
		public void AddWindow( Window toAdd )
		{
			_windows.Add( toAdd );
			if ( toAdd.Visible )
			{
				toAdd.Info.Show();
			}
		}	

		/// <summary>
		///   Removes the passed window from the virtual desktop and hides it.
		/// </summary>
		/// <param name = "toRemove">The window to remove.</param>
		public void RemoveWindow( WindowInfo toRemove )
		{
			Window window = _windows.FirstOrDefault( w => w.Info.Equals( toRemove ) );
			if ( window != null )
			{
				_windows.Remove( window );
			}
			toRemove.Hide();
		}

		/// <summary>
		///   Show all windows associated with this virtual desktop.
		/// </summary>
		internal void Show()
		{
			// Reposition windows.
			// Topmost windows are repositioned separately in order to prevent non-topmost windows from becoming topmost when moving them above topmost windows in the z-order.
			var allWindows = _windows.GroupBy( w => w.Info.IsTopmost() );
			allWindows.ForEach( group =>
			{
				var showWindows = group
					.Where( w => w.Visible )
					.Select( w => new RepositionWindowInfo( w.Info ) { Visible = true } );
				WindowManager.RepositionWindows( showWindows.ToList(), true );
			} );

			// Activate top window.
			// TODO: Is the topmost window always the previous active one? Possibly a better check is needed.
			Window first = _windows.FirstOrDefault( w => w.Visible );
			if ( first != null )
			{
				first.Info.Focus();
			}
		}

		/// <summary>
		///   Hide all windows associated with this virtual desktop.
		/// </summary>
		internal void Hide( Func<WindowInfo, List<WindowInfo>> hideBehavior )
		{
			// Find z-order of the windows.
			// TODO: Safeguard for infinite loop and possible destroyed windows.
			// http://stackoverflow.com/q/12992201/590790
			var ordenedWindows = new List<Window>();
			WindowInfo window = WindowManager.GetTopWindow();
			while ( window != null )
			{
                Window match = _windows.FirstOrDefault(w => w.Info.Equals(window));
                if (match != null)
                {
                    ordenedWindows.Add(match);
                }
				window = WindowManager.GetWindowBelow( window );
			}
			_windows = ordenedWindows;

			// Find windows to hide using process specific hide behaviors.
			var toHide = _windows.Where( w => w.Visible ).SelectMany( w => hideBehavior( w.Info ) );

			// Hide windows.
			var hideWindows = toHide.Select( w => new RepositionWindowInfo( w ) { Visible = false } );
			WindowManager.RepositionWindows( hideWindows.ToList() );
		}

		/// <summary>
		///   Serialize the current desktop to a structure, allowing to restore it at a later time.
		/// </summary>
		/// <returns>A structure holding the relevant information for this desktop.</returns>
		public StoredSession Store()
		{
			return new StoredSession( this );
		}

	}
}
