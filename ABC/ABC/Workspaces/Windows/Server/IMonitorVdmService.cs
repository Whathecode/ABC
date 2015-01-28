using System.Collections.Generic;
using Whathecode.System.Windows;


namespace ABC.Workspaces.Windows.Server
{
	/// <summary>
	///   An inter process communication service which clients can use to monitor which windows are associated to the virtual desktops.
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
	public interface IMonitorVdmService
	{
		/// <summary>
		///   Get a list of all virtual desktops, with all the windows associated to them.
		/// </summary>
		List<List<WindowInfo>> GetWindowAssociations();

		/// <summary>
		///   Get a list of all the windows on the window clipboard.
		/// </summary>
		List<WindowInfo> GetWindowClipboard();

		/// <summary>
		///   Cuts a set of windows and places them on the clipboard.
		/// </summary>
		/// <param name = "windows">The windows to cut.</param>
		void CutWindows( List<WindowInfo> windows );
	}
}
