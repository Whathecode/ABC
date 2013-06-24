using System;
using System.Collections.Generic;
using System.Diagnostics;


namespace ABC.Windows.Desktop.Settings
{
	/// <summary>
	///   Default settings which contain the correct behavior for a common set of applications.
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
	public class DefaultSettings : ISettings
	{
		readonly Func<WindowInfo, bool> _windowManagerFilter;
		readonly LoadedSettings _defaultSettings = new LoadedSettings();	// Default settings are automatically loaded.


		/// <summary>
		///   Create default settings containing the correct behavior for a common set of applications.
		///   Additionally, windows from the calling process are ignored by default, or a custom passed window filter can be used.
		/// </summary>
		public DefaultSettings( Func<WindowInfo, bool> customWindowFilter = null )
		{
			// Ignore windows created by the window manager itself.
			Process windowManagerProcess = Process.GetCurrentProcess();

			if ( customWindowFilter == null )
			{
				_windowManagerFilter = w =>
				{
					Process process = w.GetProcess();
					if ( process == null )
					{
						return false;
					}

					bool isWindowManager = process.Id == windowManagerProcess.Id;
					return !isWindowManager;
				};
			}
			else
			{
				_windowManagerFilter = customWindowFilter;
			}
		}

 
		public Func<WindowInfo, bool> CreateWindowFilter()
		{
			return w => _windowManagerFilter( w ) && _defaultSettings.CreateWindowFilter()( w );
		}

		public Func<WindowInfo, DesktopManager, List<WindowInfo>> CreateHideBehavior()
		{
			return _defaultSettings.CreateHideBehavior();
		}
	}
}
