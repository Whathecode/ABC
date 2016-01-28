using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using ABC.Workspaces.Windows.Settings.ApplicationBehavior;
using Whathecode.System.Extensions;
using Whathecode.System.Linq;
using Whathecode.System.Windows;


namespace ABC.Workspaces.Windows.Settings
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
		const string SettingsFiles = "ABC.Workspaces.Windows.Settings.ApplicationBehavior";
		readonly ApplicationBehaviors _settings = new ApplicationBehaviors();
		readonly ApplicationBehaviorsProcess _handleProcess = ApplicationBehaviorsProcess.CreateHandleProcess();
		readonly ApplicationBehaviorsProcess _dontHandleProcess = ApplicationBehaviorsProcess.CreateDontHandleProcess();
		readonly Func<Window, bool> _windowManagerFilter;

		/// <summary>
		///   Setting to determine whether windows with higher privileges than the running application should be ignored or not. False by default.
		/// </summary>
		/// <returns>True when windows with higher privileges than the running application are ignored, false otherwise.</returns>
		public bool IgnoreRequireElevatedPrivileges { get; }


		/// <summary>
		///   Create settings which can be loaded from separate setting files.
		/// </summary>
		/// <param name = "ignoreRequireElevatedPrivileges">Setting to determine whether windows with higher privileges than the running application should be ignored or not.</param>
		/// <param name = "loadDefaultSettings">Start out with default settings containing the correct behavior for a common set of applications.</param>
		/// <param name = "customWindowFilter">
		///   Windows from the calling process are ignored by default, or a custom passed window filter can be used.
		///   Returning true indicates the window needs to be handled; return false, the window will be ignored.
		/// </param>
		public LoadedSettings( bool ignoreRequireElevatedPrivileges = false, bool loadDefaultSettings = true, Func<Window, bool> customWindowFilter = null )
		{
			IgnoreRequireElevatedPrivileges = ignoreRequireElevatedPrivileges;

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

		public void AddSettingsFile( Stream stream )
		{
			using ( var xmlStream = new StreamReader( stream ) )
			{
				AddBehaviors( ApplicationBehaviors.Deserialize( xmlStream.BaseStream ) );
			}
		}

		void AddBehaviors( ApplicationBehaviors newBehaviors )
		{
			// Add new common behaviors.
			_settings.CommonIgnoreWindows.Window = newBehaviors.CommonIgnoreWindows.Window
				.Union( _settings.CommonIgnoreWindows.Window )
				.ToArray();

			// Add new or overwrite existing process behaviors.
			List<ApplicationBehaviorsProcess> processes = _settings.Process.ToList();
			foreach ( ApplicationBehaviorsProcess newProcess in newBehaviors.Process )
			{
				ApplicationBehaviorsProcess same = processes.FirstOrDefault( p => newProcess.Equals( p ) );
				if ( same != null )
				{
					processes.Remove( same );
				}
				processes.Add( newProcess );
			}
			_settings.Process = processes.ToArray();
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
				ApplicationBehaviorsProcess process = GetProcessSettings( w.WindowInfo );
				if ( !process.ShouldHandleProcess )
				{
					return false;
				}

				// Process specific settings.
				ApplicationBehavior.Window listedWindow = process.IgnoreWindows.Window.FirstOrDefault( i => i.Equals( w.WindowInfo ) );
				return process.IgnoreWindows.Mode == ApplicationBehaviorsProcessIgnoreWindowsMode.NoneExcept
					? listedWindow == null
					: listedWindow != null;
			};
		}

		public Func<Window, VirtualDesktopManager, List<Window>> CreateHideBehavior()
		{
			return ( w, m ) =>
			{
				ApplicationBehaviorsProcess process = GetProcessSettings( w.WindowInfo );
				var windows = new List<WindowInfo>();

				// Ensure at least a default setting is included.
				var hideBehaviors = new List<object>();
				if ( process == null || process.HideBehavior.Items.Length == 0 )
				{
					hideBehaviors.Add( new ApplicationBehaviorsProcessHideBehaviorDefault { Hide = ApplicationBehaviorsProcessHideBehaviorDefaultHide.AllProcessWindows } );
				}
				else
				{
					hideBehaviors = process.HideBehavior.Items.ToList();
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
		readonly Dictionary<WindowInfo, ApplicationBehaviorsProcess> _windowProcessBehaviors = new Dictionary<WindowInfo, ApplicationBehaviorsProcess>();

		ApplicationBehaviorsProcess GetProcessSettings( WindowInfo window )
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
				// Find matching settings based on file version.
				FileVersionInfo info = process.MainModule.FileVersionInfo;
				Version fileVersion = new Version( info.FileMajorPart, info.FileMinorPart, info.FileBuildPart, info.FilePrivatePart );
				var matches = _settings.Process.Where( p =>
					p.Name == process.ProcessName &&
					( p.Version == null || fileVersion.Matches( p.Version ) ) )
					.ToList();

				// Select the most optimal match, or handle the process by default when no match found.
				ApplicationBehaviorsProcess processBehavior = matches.Count == 0
					? _handleProcess
					: matches.MaxBy( p => p.Version?.Length ?? 0 );  // Longest version number that matches is most 'specific'.
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