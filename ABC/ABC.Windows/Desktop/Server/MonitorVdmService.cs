using System;
using System.Collections.Generic;
using System.Linq;
using Whathecode.System.Extensions;
using Whathecode.System.Windows.Interop;


namespace ABC.Windows.Desktop.Server
{
	/// <summary>
	///   Implementation of the inter process communication service which clients can use to monitor which windows are associated to the virtual desktops.
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
	class MonitorVdmService : MarshalByRefObject, IMonitorVdmService
	{
		readonly VirtualDesktopManager _desktopManager;


		public MonitorVdmService( VirtualDesktopManager desktopManager )
		{
			_desktopManager = desktopManager;
		}


		public List<List<WindowInfo>> GetWindowAssociations()
		{
			_desktopManager.UpdateWindowAssociations();
			return _desktopManager.Desktops.Select( d => d.WindowSnapshots.Select( w => w.Info ).ToList() ).Where( l => l.Count > 0 ).ToList();
		}

		public List<WindowInfo> GetWindowClipboard()
		{
			return _desktopManager.WindowClipboard.Select( w => w.Info ).ToList();
		}

		public void CutWindows( List<WindowInfo> windows )
		{
			List<WindowSnapshot> toCut = windows.Select( w => new WindowSnapshot( w ) ).ToList();

			// Cut the passed window from all desktops.
			// TODO: This will cause problems once windows are allowed to be on multiple desktops.
			foreach ( WindowInfo window in windows )
			{
				_desktopManager.Desktops
					.Where( d => d.WindowSnapshots.Any( w => w.Info.Equals( window ) ) )
					.ForEach( d => d.RemoveWindows( toCut ) );
			}

			// Add windows to the clipboard.
			toCut.ForEach( w => _desktopManager.WindowClipboard.Push( w ) );
		}

		public override object InitializeLifetimeService()
		{
			// Guarantee infinite lifetime.
			return null;
		}
	}
}
