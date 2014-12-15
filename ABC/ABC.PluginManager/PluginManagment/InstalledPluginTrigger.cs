using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ABC.Applications.Persistence;
using ABC.Common;
using ABC.Interruptions;
using ABC.Windows.Desktop.Settings;
using PluginManager.Model;


namespace PluginManager.PluginManagment
{
	class InstalledPluginTrigger
	{
		public List<Plugin> InstalledPlugins { get; private set; }
		readonly LoadedSettings _vdmSettings;

		public InstalledPluginTrigger()
		{
			InstalledPlugins = new List<Plugin>();
			_vdmSettings = new LoadedSettings( true );

			// Add all plug-ins from target directories to installed applications. 
			AddInterruption( App.InterruptionsPluginLibrary );
			AddPersistence( App.PersistencePluginLibrary );
			AddVdm( App.VdmPluginLibrary );
		}

		void AddInterruption( string path )
		{
			var interruptionAggregator = new InterruptionAggregator( path );
			var interruptionNumber = 0;
			var dllsPaths = Directory.EnumerateFiles( path, "*.dll" ).ToList();
			interruptionAggregator.GetInterruptionInfos().ForEach( interruption =>
			{
				var app = CheckIfExists( interruption );

				AddConfiguration( app.Interruptions, interruption, dllsPaths, interruptionNumber );
				++interruptionNumber;
			} );
		}

		void AddPersistence( string path )
		{
			var persistenceProvider = new PersistenceProvider( path );
			var persistenceNumber = 0;
			var dllsPaths = Directory.EnumerateFiles( path, "*.dll" ).ToList();
			persistenceProvider.GetPersistenceProvidersInfo().ForEach( persistance =>
			{
				var app = CheckIfExists( persistance );
				AddConfiguration( app.Persistence, persistance, dllsPaths, persistenceNumber );
				++persistenceNumber;
			} );
			persistenceProvider.Dispose();
		}

		Plugin CheckIfExists( PluginInformation pluginInfo )
		{
			var plugin = InstalledPlugins.FirstOrDefault( installedApp => installedApp.Name == pluginInfo.ProcessName );
			if ( plugin != null ) return plugin;

			plugin = new Plugin
			{
				Name = pluginInfo.ProcessName,
				CompanyName = pluginInfo.CompanyName,
			};
			AddPlugin( plugin );

			return plugin;
		}

		static void AddConfiguration( ICollection<Configuration> pluginType, PluginInformation persistance, List<string> dllsPaths, int pluginNumber )
		{
			pluginType.Add( new Configuration
			{
				SupportedVersions = persistance.SupportedVersions,
				Author = persistance.Author,
				Version = GetDllVersion( dllsPaths[ pluginNumber ] ),
				TimeStamp = GetLastWriteDate( dllsPaths[ pluginNumber ] )
			} );
		}

		/// <summary>
		/// Adds VDM configurations from given directory.
		/// </summary>
		void AddVdm( string path )
		{
			// Add VDM configurations from plug-in installation directory
			var plugins = Directory.EnumerateFiles( path, "*.xml" );
			foreach ( var plugin in plugins )
			{
				try
				{
					using ( var stream = new FileStream( plugin, FileMode.Open ) )
					{
						_vdmSettings.AddSettingsFile( stream );
					}
				}
				catch ( InvalidOperationException )
				{
					// Simply ignore invalid files.
				}
			}

			_vdmSettings.Settings.Process.ForEach( vdmCfg =>
			{
				// Check if application with the same name already is not installed.
				var app = InstalledPlugins.FirstOrDefault( installedApp => installedApp.Name == vdmCfg.Name && installedApp.Author == vdmCfg.CompanyName );
				if ( app == null )
				{
					app = new Plugin
					{
						Name = vdmCfg.Name,
						CompanyName = vdmCfg.CompanyName
					};
					AddPlugin( app );
				}
				var supportedVersions = vdmCfg.Version != null ? vdmCfg.Version.Split( ',' ).Select( sv => sv.Trim() ).ToList() : new List<string>();
				app.Vdm.Add( new Configuration
				{
					Name = vdmCfg.Name,
					Author = vdmCfg.Author,
					SupportedVersions = supportedVersions
				} );
			} );
		}

		void AddPlugin( Plugin app )
		{
			if ( InstalledPlugins.Contains( app ) )
			{
				return;
			}
			InstalledPlugins.Add( app );
		}

		static string GetDllVersion( string dllPath )
		{
			return new AssemblyInfo( Assembly.LoadFile( dllPath ) ).Version;
		}

		static string GetLastWriteDate( string dllPath )
		{
			return new FileInfo( dllPath ).LastWriteTime.ToShortDateString();
		}
	}
}