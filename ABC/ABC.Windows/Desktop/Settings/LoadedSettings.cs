using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Generated.ProcessBehaviors;


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
		const string DefaultSettingsFile = "ABC.Windows.Desktop.Settings.ProcessBehavior.default_settings.xml";
		ProcessBehaviors _settings;


		public LoadedSettings( bool loadDefaultSettings = true )
		{
			if ( loadDefaultSettings )
			{
				Stream settingsStream = Assembly.GetExecutingAssembly().GetManifestResourceStream( DefaultSettingsFile );
				AddSettingsFile( settingsStream );
			}
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

		public Func<WindowInfo, bool> CreateWindowFilter()
		{
			return w =>
			{
				// Common windows to filter.
				if ( _settings.CommonIgnoreWindows.Window.FirstOrDefault( i => i.Equals( w ) ) != null )
				{
					return false;
				}

				// Process specific settings.
				ProcessBehaviorsProcess process = GetProcessSettings( w );
				return
					process == null ||
					process.IgnoreWindows.Window.FirstOrDefault( i => i.Equals( w ) ) == null;
			};
		}

		public Func<WindowInfo, DesktopManager, List<WindowInfo>> CreateHideBehavior()
		{
			return (w, m) =>
			{
				ProcessBehaviorsProcess process = GetProcessSettings( w );
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
					windows.AddRange( hideBehavior.ToCut( w, m ) );
				}

				return windows.Distinct().ToList();
			};
		}

		readonly List<WindowInfo> _accessDeniedWindows = new List<WindowInfo>();
		readonly Dictionary<WindowInfo, ProcessBehaviorsProcess> _windowProcessBehaviors = new Dictionary<WindowInfo, ProcessBehaviorsProcess>();
		ProcessBehaviorsProcess GetProcessSettings( WindowInfo window )
		{
			// See whether settings are cached.
			if ( _accessDeniedWindows.Contains( window ) )
			{
				return null;
			}
			if ( _windowProcessBehaviors.ContainsKey( window ) )
			{
				return _windowProcessBehaviors[ window ];
			}
			
			// Prevent cached settings from being kept in memory when windows are destroyed.
			var deniedToRemove = _accessDeniedWindows.Where( w => w.IsDestroyed() ).ToArray();
		    foreach (var windowInfo in deniedToRemove)
		    {
		        _accessDeniedWindows.Remove(windowInfo);
		    }
			var processBehaviorsToRemove = _windowProcessBehaviors.Where( p => p.Key.IsDestroyed() ).ToArray();
		    foreach (var keyValuePair in processBehaviorsToRemove)
		    {
		        _windowProcessBehaviors.Remove(keyValuePair.Key);
		    }

			// Get settings.
			Process process = window.GetProcess();
			try
			{
				FileVersionInfo versionInfo = process.MainModule.FileVersionInfo;

				var matches = _settings.Process.Where( p =>
					p.Name == process.ProcessName &&
					p.CompanyName == versionInfo.CompanyName &&
					( p.Version == null || versionInfo.FileVersion.StartsWith( p.Version ) ) ).ToList();

				ProcessBehaviorsProcess processBehavior = matches.Count == 0
					? null
					: MaxBy(matches);
				_windowProcessBehaviors[ window ] = processBehavior;

				return processBehavior;
			}
			catch ( Win32Exception )
			{
				_accessDeniedWindows.Add( window );
				return null;
			}
		}

	    private ProcessBehaviorsProcess MaxBy(List<ProcessBehaviorsProcess> matches)
	    {
	        ProcessBehaviorsProcess processMaxCount = null;
	        foreach (var p in matches)
	        {
	            if (processMaxCount == null)
	                processMaxCount = p;
	            if (p.Version.Length > processMaxCount.Version.Length)
	                processMaxCount = p;

	            return processMaxCount;
	        }
	    }
	}
}
