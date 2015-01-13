using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ABC.Applications.Persistence;
using ABC.Interruptions;
using ABC.Windows.Desktop.Settings;
using PluginManager.common;
using PluginManager.Common;
using PluginManager.Model;
using Whathecode.System;


namespace PluginManager.PluginManagment
{
	public class InstalledPluginManager : AbstractDisposable
	{
		readonly LoadedSettings _vdmSettings;
		InterruptionAggregator _interruptionAggregator;
		PersistenceProvider _persistenceProvider;

		public List<Plugin> PersistencePlugins { get; private set; }
		public List<Plugin> InterruptionsPlugins { get; private set; }
		public List<Plugin> VdmPlugins { get; private set; }

		public InstalledPluginManager()
		{
			_vdmSettings = new LoadedSettings( true );
			_interruptionAggregator = new InterruptionAggregator( App.InterruptionsPluginLibrary );
			_persistenceProvider = new PersistenceProvider( App.PersistencePluginLibrary );
			RefreshPlugins();
		}

		public void RefreshPlugins()
		{
			PersistencePlugins = GetPersistence();
			InterruptionsPlugins = GetInterruptions();
			VdmPlugins = GetVdms();
		}

		List<Plugin> GetInterruptions()
		{
			var interruptionPlugins = new List<Plugin>();
			var interruptionNumber = 0;
			var dllsPaths = Directory.EnumerateFiles( App.InterruptionsPluginLibrary, "*.dll" ).ToList();
			_interruptionAggregator.GetInterruptionInfos().ForEach( interruption =>
			{
				var app = GetOrCreate( interruption.ProcessName, interruption.CompanyName, interruptionPlugins );
				app.Interruptions.Add( CreateConfiguration( interruption.SupportedVersions,
					interruption.Author, FileHelper.GetDllVersion( dllsPaths[ interruptionNumber ] ),
					FileHelper.GetLastWriteDate( dllsPaths[ interruptionNumber ] ) ) );
				++interruptionNumber;
			} );
			PluginManagmentHelper.SortByName( ref interruptionPlugins );
			return interruptionPlugins;
		}

		List<Plugin> GetPersistence()
		{
			//_persistenceProvider = new PersistenceProvider( App.PersistencePluginLibrary );
			var persistencePlugins = new List<Plugin>();
			var persistenceNumber = 0;
			var dllsPaths = Directory.EnumerateFiles( App.PersistencePluginLibrary, "*.dll" ).ToList();
			_persistenceProvider.GetPersistenceProvidersInfo().ForEach( persistance =>
			{
				var app = GetOrCreate( persistance.ProcessName, persistance.CompanyName, persistencePlugins );
				app.Persistence.Add( CreateConfiguration( persistance.SupportedVersions,
					persistance.Author, FileHelper.GetDllVersion( dllsPaths[ persistenceNumber ] ),
					FileHelper.GetLastWriteDate( dllsPaths[ persistenceNumber ] ) ) );
				++persistenceNumber;
			} );
			PluginManagmentHelper.SortByName( ref persistencePlugins );
			// ???
			//_persistenceProvider.Dispose();
			return persistencePlugins;
		}

		List<Plugin> GetVdms()
		{
			var vdms = new List<Plugin>();
			// Add VDM configurations from plug-in installation directory
			var plugins = Directory.EnumerateFiles( App.VdmPluginLibrary, "*.xml" );
			foreach ( var plugin in plugins )
			{
				try
				{
					using ( var stream = new FileStream( plugin, FileMode.Open ) )
					{
						_vdmSettings.AddSettingsFile( stream );
					}
				}
				catch ( InvalidOperationException ) {}
			}

			_vdmSettings.Settings.Process.ForEach( vdmCfg =>
			{
				// Check if application with the same name already is not installed.
				var app = GetOrCreate( vdmCfg.Name, vdmCfg.CompanyName, vdms );
				var supportedVersions = vdmCfg.Version != null ? vdmCfg.Version.Split( ',' ).Select( sv => sv.Trim() ).ToList() : new List<string>();
				app.Vdm.Add( CreateConfiguration( supportedVersions, vdmCfg.Name, vdmCfg.Version, DateTime.Now.ToShortDateString() ) );
			} );
			PluginManagmentHelper.SortByName( ref vdms );
			return vdms;
		}

		public void InstallPlugin( string name, string companyName, PluginType pluginType )
		{
			if ( pluginType == PluginType.Persistence )
			{
				_persistenceProvider.Reload();
				_persistenceProvider.InstallPlugin( name, companyName );
			}
		}

		Plugin GetOrCreate( string name, string companyName, List<Plugin> plugins )
		{
			var plugin = plugins.FirstOrDefault( installedApp => installedApp.Name == name && installedApp.CompanyName == companyName );
			if ( plugin != null )
			{
				return plugin;
			}

			plugin = new Plugin
			{
				Name = name,
				CompanyName = companyName,
			};
			plugins.Add( plugin );
			return plugin;
		}

		Configuration CreateConfiguration( List<string> supportedVersions, string author, string version, string timeStamp )
		{
			return new Configuration
			{
				SupportedVersions = supportedVersions,
				Author = author,
				Version = version,
				TimeStamp = timeStamp,
				State = PluginState.Installed
			};
		}

		protected override void FreeManagedResources()
		{
			_persistenceProvider.Dispose();
		}

		protected override void FreeUnmanagedResources()
		{
			// Nothing to do.
		}
	}
}