using System;
using System.Collections.Generic;
using ABC.Applications.Persistence;
using ABC.Windows.Desktop.Settings;
using Whathecode.System;


namespace ABC.Windows.Desktop
{
	/// <summary>
	///   Allows creating and switching between different <see cref="VirtualDesktop" />'s 
	///   together with associated desktop icons.
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
	public class WorkspaceManager : AbstractDisposable
	{
		readonly bool _storeDesktopIcons;
		readonly VirtualDesktopManager _desktopManager;

		public VirtualDesktop StartupDesktop
		{
			get { return _desktopManager.StartupDesktop; }
		}

		public VirtualDesktop CurrentDesktop
		{
			get { return _desktopManager.CurrentDesktop; }
		}

		public IReadOnlyCollection<VirtualDesktop> Desktops
		{
			get { return _desktopManager.Desktops; }
		}

		/// <summary>
		///   Initializes a new desktop manager and creates one startup desktop containing all currently open windows.
		///   This desktop is accessible through the <see cref="CurrentDesktop" /> property.
		/// </summary>
		/// <param name = "settings">Contains settings for how the desktop manager should behave. E.g. which windows to ignore.</param>
		/// <param name = "storeDesktopIcons">Determines whether desktop icons and its layout should be stored individually for every virtual desktop. True by default.</param>
		public WorkspaceManager( ISettings settings, bool storeDesktopIcons = true )
			: this( settings, new CollectionPersistenceProvider(), storeDesktopIcons ) {}

		/// <summary>
		///   Initializes a new desktop manager and creates one startup desktop containing all currently open windows.
		///   This desktop is accessible through the <see cref="CurrentDesktop" /> property.
		/// </summary>
		/// <param name = "settings">Contains settings for how the desktop manager should behave. E.g. which windows to ignore.</param>
		/// <param name = "persistenceProvider">Allows state of applications to be persisted and restored.</param>
		/// <param name = "storeDesktopIcons">Determines whether desktop icons and its layout should be stored individually for every virtual desktop. True by default.</param>
		public WorkspaceManager( ISettings settings, AbstractPersistenceProvider persistenceProvider, bool storeDesktopIcons = true )
		{
			_desktopManager = new VirtualDesktopManager( settings, persistenceProvider )
			{
				// Set folder for the startup desktop.
				StartupDesktop = { Folder = Environment.GetFolderPath( Environment.SpecialFolder.Desktop ) }
			};
			_storeDesktopIcons = storeDesktopIcons;
		}

		/// <summary>
		///   Create an empty virtual desktop with no windows assigned to it.
		/// </summary>
		/// <param name = "folder">The folder associated with this desktop, which is used to populate the desktop icons.</param>
		/// <returns>The newly created virtual desktop.</returns>
		public VirtualDesktop CreateEmptyDesktop( string folder = null )
		{
			var desktop = _desktopManager.CreateEmptyDesktop();
			desktop.Folder = folder ?? Environment.GetFolderPath( Environment.SpecialFolder.Desktop );
			return desktop;
		}

		/// <summary>
		///   Creates a new desktop from a stored session.
		/// </summary>
		/// <param name = "session">The newly created virtual desktop.</param>
		/// <param name = "folder">The folder associated with this desktop, which is used to populate the desktop icons.</param>
		/// <returns>The restored virtual desktop.</returns>
		public VirtualDesktop CreateDesktopFromSession( StoredSession session, string folder = null )
		{
			throw new NotSupportedException();
		}

		/// <summary>
		///   Update the icons associated to the current virtual desktop.
		/// </summary>
		public void SwitchDesktopIcons( VirtualDesktop targetDesktop )
		{
			if ( !_storeDesktopIcons ) return;

			// Store the icons.
			_desktopManager.CurrentDesktop.Icons = IconManager.SaveDestopIcons();

			// Update targetDesktop icons.
			IconManager.ChangeDesktopFolder( targetDesktop.Folder );
			IconManager.ArrangeDesktopIcons( targetDesktop.Icons );
		}

		/// <summary>
		///   Update which windows are associated to the current virtual desktop.
		/// </summary>
		public void UpdateWindowAssociations()
		{
			_desktopManager.UpdateWindowAssociations();
		}


		/// <summary>
		///   Switch to the given virtual desktop.
		/// </summary>
		/// <param name="desktop">The desktop to switch to.</param>
		public void SwitchToDesktop( VirtualDesktop desktop )
		{
			try
			{
				_desktopManager.SwitchToDesktop( desktop );
			}
			finally
			{
				SwitchDesktopIcons( desktop );
			}
		}

		/// <summary>
		///   Merges all windows from one desktop with those from another, and removes the original desktop.
		///   You can't merge the <see cref="StartupDesktop"/> with another desktop.
		/// </summary>
		/// <returns>A new virtual desktop which has windows of both passed desktops assigned to it.</returns>
		public void Merge( VirtualDesktop from, VirtualDesktop to )
		{
			_desktopManager.Merge( from, to );
			// TODO: Modify method to support icon merge.
		}

		/// <summary>
		///   Closes the virtual desktop manager by restoring all windows.
		/// </summary>
		public void Close()
		{
			_desktopManager.Close();
		}

		/// <summary>
		///   Cut a given window from the currently open desktop and store it in a clipboard.
		///   TODO: What if a window from a different desktop is passed? Should this be supported?
		/// </summary>
		public void CutWindow( Window window )
		{
			_desktopManager.CutWindow( window );
		}

		/// <summary>
		///   Paste all windows on the clipboard on the currently open desktop.
		/// </summary>
		public void PasteWindows()
		{
			_desktopManager.PasteWindows();
		}


		protected override void FreeManagedResources()
		{
			_desktopManager.Close();
		}

		protected override void FreeUnmanagedResources()
		{
			// Nothing to do.
		}
	}
}