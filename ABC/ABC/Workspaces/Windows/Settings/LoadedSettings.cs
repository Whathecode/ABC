using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using ABC.Plugins;
using ABC.Workspaces.Windows.Settings.ProcessBehavior;
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
	public class LoadedSettings : ISettings, IInstallablePluginContainer
	{
		const string SettingsFiles = "ABC.Workspaces.Windows.Settings.ProcessBehavior";
		public ProcessBehaviors Settings { get; private set; }
		readonly ProcessBehaviorsProcess _handleProcess = new ProcessBehaviorsProcess();
		readonly ProcessBehaviorsProcess _dontHandleProcess = ProcessBehaviorsProcess.CreateDontHandleProcess();
		readonly Func<Window, bool> _windowManagerFilter;

		readonly List<WindowInfo> _accessDeniedWindows = new List<WindowInfo>();
		readonly Dictionary<WindowInfo, ProcessBehaviorsProcess> _windowProcessBehaviors = new Dictionary<WindowInfo, ProcessBehaviorsProcess>();
		readonly bool _loadDefaultSettings;

		IEnumerable<string> _loadedFiles = new List<string>();

		public bool IgnoreRequireElevatedPrivileges { get; private set; }
		
		public string PluginFolderPath { get; private set; }

		/// <summary>
		///   Create settings which can be loaded from separate setting files.
		/// </summary>
		/// <param name = "pluginFolderPath">Path to the external configurations directory.</param>
		/// <param name = "ignoreRequireElevatedPrivileges">Setting to determine whether windows with higher privileges than the running application should be ignored or not.</param>
		/// <param name = "loadDefaultSettings">Start out with default settings containing the correct behavior for a common set of applications.</param>
		/// <param name = "customWindowFilter">Windows from the calling process are ignored by default, or a custom passed window filter can be used.</param>
		public LoadedSettings( string pluginFolderPath, bool ignoreRequireElevatedPrivileges = false, bool loadDefaultSettings = false, Func<Window, bool> customWindowFilter = null )
		{
			IgnoreRequireElevatedPrivileges = ignoreRequireElevatedPrivileges;

			_loadDefaultSettings = loadDefaultSettings;
			if ( loadDefaultSettings )
			{
				LoadDefaultSetting();
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

			PluginFolderPath = pluginFolderPath;
			LoadSettingsFromPath( PluginFolderPath );

		}

		/// <summary>
		/// Reloads all settings.
		/// </summary>
		public void Refresh()
		{
			Settings = null;
			if ( _loadDefaultSettings )
			{
				LoadDefaultSetting();
			}

			LoadSettingsFromPath( PluginFolderPath );
		}

		void LoadSettingsFromPath( string settingsFolderPath )
		{
			_loadedFiles = Directory.EnumerateFiles( settingsFolderPath, "*.xml" );
			foreach ( var plugin in _loadedFiles )
			{
				try
				{
					using ( var stream = new FileStream( plugin, FileMode.Open ) )
					{
						AddSettingsFile( stream );
					}
				}
				catch ( InvalidOperationException ) {}
			}
		}

		void LoadDefaultSetting()
		{
			var assembly = Assembly.GetExecutingAssembly();
			assembly
				.GetManifestResourceNames()
				.Where( name => name.StartsWith( SettingsFiles ) )
				.ForEach( settingsFile => AddSettingsFile( assembly.GetManifestResourceStream( settingsFile ) ) );
		}

		public void AddSettingsFile( Stream stream )
		{
			using ( var xmlStream = new StreamReader( stream ) )
			{
				AddBehaviors( ProcessBehaviors.Deserialize( xmlStream.BaseStream ) );
			}
		}

		void AddBehaviors( ProcessBehaviors newBehaviors )
		{
			if ( Settings == null )
			{
				Settings = newBehaviors;
				return;
			}

			// Add new common behaviors.
			newBehaviors.CommonIgnoreWindows.Window
				.ForEach( newCommon => Settings.CommonIgnoreWindows.AddIfAbsent( newCommon ) );

			// Add new or overwrite existing process behaviors.
			newBehaviors.Process.ForEach( newProcess => Settings.AddOrOverwriteProcess( newProcess ) );
		}

		public Func<Window, bool> CreateWindowFilter()
		{
			return w =>
			{
				// Custom filter and common windows to filter.
				if ( !_windowManagerFilter( w ) || Settings.CommonIgnoreWindows.Window.FirstOrDefault( i => i.Equals( w.WindowInfo ) ) != null )
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
				ProcessBehavior.Window listedWindow = process.IgnoreWindows.Window.FirstOrDefault( i => i.Equals( w.WindowInfo ) );
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
				if ( process == null || process.HideBehavior.Items.Length == 0 )
				{
					hideBehaviors.Add( new ProcessBehaviorsProcessHideBehaviorDefault { Hide = ProcessBehaviorsProcessHideBehaviorDefaultHide.AllProcessWindows } );
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

				// Find configuration for given process name.
				var matches = Settings.Process.Where( p =>
					p.TargetProcessName == process.ProcessName ).ToList();

				// Check if there are any version specific configurations for given process.
				var versionSpecificMatches = matches.Where( match =>
					match.TargetProcessVersionHelper.Major == versionInfo.FileMajorPart &&
					match.TargetProcessVersionHelper.Minor == versionInfo.FileMinorPart ).ToList();

				// If any use version specific configurations, application general otherwise.
				matches = versionSpecificMatches.Any()
					? versionSpecificMatches
					: matches.Where( match => match.IsGeneral )
						.ToList();

				// Take a default configuration if nothing matched, otherwise one with the highest version.
				ProcessBehaviorsProcess processBehavior = matches.Count == 0
					? _handleProcess
					: matches.MaxBy( p => p.VersionHelper );
				_windowProcessBehaviors[ window ] = processBehavior;

				return processBehavior;
			}
			catch ( Win32Exception )
			{
				_accessDeniedWindows.Add( window );
				return _dontHandleProcess;
			}
		}

		public IInstallable GetInstallablePlugin( Guid guid )
		{
			// TODO: How to make VDM configuration installable?
			var plugin = GetProcessBehaviorsProcess( guid ) as IInstallable;
			return plugin;
		}

		public Version GetPluginVersion( Guid guid )
		{
			var plugin = GetProcessBehaviorsProcess( guid );
			return plugin != null ? new Version( plugin.Version ) : null;
		}

		public string GetPluginPath( Guid guid )
		{
			return _loadedFiles.FirstOrDefault( pluginPath => pluginPath.IndexOf( guid.ToString(), StringComparison.OrdinalIgnoreCase ) >= 0 );
		}

		ProcessBehaviorsProcess GetProcessBehaviorsProcess( Guid guid )
		{
			return Settings.Process.FirstOrDefault( process => new Guid( process.Guid ) == guid );
		}
	}
}