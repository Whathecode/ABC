using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Generated.ProcessBehaviors;
using Whathecode.System.Extensions;
using Whathecode.System.Linq;
using Whathecode.System.Windows.Interop;


namespace ABC.Windows.Desktop.Settings
{
	/// <summary>
	///   Settings which allow loading external setting files.
	///   Newly loaded setting are merged with previously loaded settings, or overridden when they already exist.
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
	public class LoadedSettings : ISettings
	{
		const string SettingsFiles = "ABC.Windows.Desktop.Settings.ProcessBehavior";
		ProcessBehaviors _settings;
		readonly ProcessBehaviorsProcess _handleProcess = new ProcessBehaviorsProcess();
		readonly ProcessBehaviorsProcess _dontHandleProcess = ProcessBehaviorsProcess.CreateDontHandleProcess();
		readonly Func<Window, bool> _windowManagerFilter;
		readonly bool _ignoreRequireElevatedPrivileges;


		/// <summary>
		///   Create settings which can be loaded from separate setting files.
		/// </summary>
		/// <param name = "ignoreRequireElevatedPrivileges">Setting to determine whether windows with higher privileges than the running application should be ignored or not.</param>
		/// <param name = "loadDefaultSettings">Start out with default settings containing the correct behavior for a common set of applications.</param>
		/// <param name = "customWindowFilter">Windows from the calling process are ignored by default, or a custom passed window filter can be used.</param>
		public LoadedSettings( bool ignoreRequireElevatedPrivileges = false, bool loadDefaultSettings = true, Func<Window, bool> customWindowFilter = null )
		{
			_ignoreRequireElevatedPrivileges = ignoreRequireElevatedPrivileges;

			if ( loadDefaultSettings )
			{
				Assembly assembly = Assembly.GetExecutingAssembly();
				assembly
					.GetManifestResourceNames()
					.Where( name => name.StartsWith( SettingsFiles ) )
					.ForEach( settingsFile => AddSettingsFile( assembly.GetManifestResourceStream( settingsFile ) ) );
			}

			// Ignore windows created by the window manager itself when no filter is specified.
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


		public bool IgnoreRequireElevatedPrivileges
		{
			get { return _ignoreRequireElevatedPrivileges; }
		}


		public void AddSettingsFile( Stream stream )
		{
			using ( var xmlStream = new StreamReader( stream ) )
			{
				AddBehaviors( ProcessBehaviors.Deserialize( xmlStream.ReadToEnd() ) );
			}
		}

		void AddBehaviors( ProcessBehaviors newBehaviors )
		{
			if ( _settings == null )
			{
				_settings = newBehaviors;
				return;
			}

			// Add new common behaviors.
			foreach ( var newCommon in newBehaviors.CommonIgnoreWindows.Window )
			{
				if ( _settings.CommonIgnoreWindows.Window.FirstOrDefault( i => i.Equals( newCommon ) ) == null )
				{
					_settings.CommonIgnoreWindows.Window.Add( newCommon );
				}
			}

			// Add new or overwrite existing process behaviors.
			foreach ( var newProcess in newBehaviors.Process )
			{
				var same = _settings.Process.FirstOrDefault( p => newProcess.Equals( p ) );
				if ( same != null )
				{
					_settings.Process.Remove( same );
				}
				_settings.Process.Add( newProcess );
			}
		}

		public Func<Window, bool> CreateWindowFilter()
		{
			return w =>
			{
				// Custom filter and common windows to filter.
				if ( !_windowManagerFilter( w ) || _settings.CommonIgnoreWindows.Window.FirstOrDefault( i => i.Equals( w.WindowInfo ) ) != null )
				{
					return false;
				}

				// Check whether the process needs to be managed at all.
				ProcessBehaviorsProcess process = GetProcessSettings( w.WindowInfo );
				if ( !process.ShouldHandleProcess() )
				{
					return false;
				}

				// Process specific settings.
				Generated.ProcessBehaviors.Window listedWindow = process.IgnoreWindows.Window.FirstOrDefault( i => i.Equals( w.WindowInfo ) );
				return process.IgnoreWindows.Mode == ProcessBehaviorsProcessIgnoreWindowsMode.NoneExcept
					? listedWindow == null
					: listedWindow != null;
			};
		}

		public Func<Window, VirtualDesktopManager, List<Window>> CreateHideBehavior()
		{
			return ( w, m ) =>
			{
				ProcessBehaviorsProcess process = GetProcessSettings( w.WindowInfo );
				var windows = new List<WindowInfo>();

				// Ensure at least a default setting is included.
				var hideBehaviors = new List<object>();
				if ( process == null || process.HideBehavior.Items.Count == 0 )
				{
					hideBehaviors.Add( new ProcessBehaviorsProcessHideBehaviorDefault { Hide = ProcessBehaviorsProcessHideBehaviorDefaultHide.AllProcessWindows } );
				}
				else
				{
					hideBehaviors = process.HideBehavior.Items;
				}

				// Go through all specified cut behaviors in order.
				foreach ( var hideBehavior in hideBehaviors.Cast<ICutBehavior>() )
				{
					windows.AddRange( hideBehavior.ToCut( w.WindowInfo, m ) );
				}

				return windows.Distinct().Select( wi => new Window( wi ) ).ToList();
			};
		}

		readonly List<WindowInfo> _accessDeniedWindows = new List<WindowInfo>();
		readonly Dictionary<WindowInfo, ProcessBehaviorsProcess> _windowProcessBehaviors = new Dictionary<WindowInfo, ProcessBehaviorsProcess>();
		ProcessBehaviorsProcess GetProcessSettings( WindowInfo window )
		{
			// See whether settings are cached.
			if ( _accessDeniedWindows.Contains( window ) )
			{
				return _dontHandleProcess;
			}
			if ( _windowProcessBehaviors.ContainsKey( window ) )
			{
				return _windowProcessBehaviors[ window ];
			}

			// Prevent cached settings from being kept in memory when windows are destroyed.
			var deniedToRemove = _accessDeniedWindows.Where( w => w.IsDestroyed() ).ToArray();
			deniedToRemove.ForEach( w => _accessDeniedWindows.Remove( w ) );
			var processBehaviorsToRemove = _windowProcessBehaviors.Where( p => p.Key.IsDestroyed() ).ToArray();
			processBehaviorsToRemove.ForEach( p => _windowProcessBehaviors.Remove( p.Key ) );

			// Get settings.
			Process process = window.GetProcess();
			
			try
			{
				FileVersionInfo versionInfo = process.MainModule.FileVersionInfo;

				var matches = _settings.Process.Where( p =>
					p.Name == process.ProcessName &&
					( p.Version == null || versionInfo.FileVersion.StartsWith( p.Version ) ) ).ToList();

				ProcessBehaviorsProcess processBehavior = matches.Count == 0
					? _handleProcess
					: matches.MaxBy( p => p.Version == null ? 0 : p.Version.Length );
				_windowProcessBehaviors[ window ] = processBehavior;

				return processBehavior;
			}
			catch ( Win32Exception )
			{
				_accessDeniedWindows.Add( window );
				return _dontHandleProcess;
			}
		}
	}
}
