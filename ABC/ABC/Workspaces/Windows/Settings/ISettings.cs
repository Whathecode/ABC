using System;
using System.Collections.Generic;


namespace ABC.Workspaces.Windows.Settings
{
	/// <summary>
	///   Contains settings for the desktop manager, on how it should behave. E.g. which windows to ignore.
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
	public interface ISettings
	{
		/// <summary>
		///   Setting to determine whether windows with higher privileges than the running application should be ignored or not.
		/// </summary>
		/// <returns>True when windows with higher privileges than the running application are ignored, false otherwise.</returns>
		bool IgnoreRequireElevatedPrivileges { get; }

		/// <summary>
		///   Creates a filter which returns true if the given window should be managed by the desktop manager, false otherwise.
		/// </summary>
		/// <returns></returns>
		Func<Window, bool> CreateWindowFilter();

		/// <summary>
		///   Creates a method which determines which windows should be cut when a given window is cut.
		/// </summary>
		/// <returns>The list of windows which should be cut when the given window is cut.</returns>
		Func<Window, VirtualDesktopManager, List<Window>> CreateHideBehavior();
	}
}
