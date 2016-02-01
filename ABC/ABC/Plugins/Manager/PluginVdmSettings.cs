using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ABC.Workspaces.Windows;
using ABC.Workspaces.Windows.Settings;


namespace ABC.Plugins.Manager
{
	/// <summary>
	///   Contains settings for the VDM manager which are loaded and managed by the plugin manager.
	///   TODO: This is a naive copy/paste of some of plugin manager logic previously embedded within 'LoadedSettings'.
	///         I split this up, but did not give it much thought. This probably needs to be written from scratch.
	/// </summary>
	public class PluginVdmSettings : ISettings, IInstallablePluginContainer
	{
		IEnumerable<string> _loadedFiles = new List<string>();
		LoadedSettings _settings;
		Func<Window, bool> _windowFilter;
		Func<Window, VirtualDesktopManager, List<Window>> _hideBehavior;

		public string PluginFolderPath { get; }

		public bool IgnoreRequireElevatedPrivileges { get; }


		/// <summary>
		///   Create VDM settings which are loaded and managed by the plugin manager.
		/// </summary>
		/// <param name = "pluginFolderPath">Path to the external configurations directory.</param>
		/// <param name = "ignoreRequireElevatedPrivileges">Setting to determine whether windows with higher privileges than the running application should be ignored or not.</param>
		public PluginVdmSettings( string pluginFolderPath, bool ignoreRequireElevatedPrivileges = false )
		{
			PluginFolderPath = pluginFolderPath;
			IgnoreRequireElevatedPrivileges = ignoreRequireElevatedPrivileges;

			Refresh();
		}


		public Func<Window, bool> CreateWindowFilter()
		{
			return w =>
			{
				if ( _windowFilter == null )
				{
					return true;
				}

				return _windowFilter( w );
			};
		}

		public Func<Window, VirtualDesktopManager, List<Window>> CreateHideBehavior()
		{
			return (w, m) =>
			{
				if ( _hideBehavior == null )
				{
					return new List<Window>();
				}

				return _hideBehavior( w, m );
			};
		}

		public void Refresh()
		{
			_settings = new LoadedSettings( IgnoreRequireElevatedPrivileges, false ); // No default settings are loaded.
			_loadedFiles = Directory.EnumerateFiles( PluginFolderPath, "*.xml" );
			foreach ( var plugin in _loadedFiles )
			{
				using ( var stream = new FileStream( plugin, FileMode.Open ) )
				{
					_settings.AddSettingsFile( stream );
				}
			}
			_windowFilter = _settings.CreateWindowFilter();
			_hideBehavior = _settings.CreateHideBehavior();
		}

		public IInstallable GetInstallablePlugin( Guid guid )
		{
			throw new NotImplementedException();
		}

		public Version GetPluginVersion( Guid guid )
		{
			throw new NotImplementedException();
		}

		public string GetPluginPath( Guid guid )
		{
			return _loadedFiles.FirstOrDefault( pluginPath => pluginPath.IndexOf( guid.ToString(), StringComparison.OrdinalIgnoreCase ) >= 0 );
		}
	}
}