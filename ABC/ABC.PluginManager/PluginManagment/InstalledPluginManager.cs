using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using ABC.Applications.Persistence;
using ABC.Common;
using ABC.Interruptions;
using ABC.Windows.Desktop.Settings;
using PluginManager.Common;
using PluginManager.Model;
using Whathecode.System;


namespace PluginManager.PluginManagment
{
	public class InstalledPluginManager : AbstractDisposable
	{
		public delegate void PluginManagerEventHandler( string message, Plugin plugin );

		/// <summary>
		///   Event which is triggered when plug-in cannot be loaded.
		/// </summary>
		public event PluginManagerEventHandler PluginCompositionFailEvent;

		/// <summary>
		///   Event which is triggered when plug-in installation processes has ended.
		/// </summary>
		public event PluginManagerEventHandler PluginInstalledEvent;

		LoadedSettings _vdmSettings;
		InterruptionAggregator _interruptionAggregator;
		PersistenceProvider _persistenceProvider;

		public List<Plugin> PersistencePlugins { get; private set; }
		public List<Plugin> InterruptionsPlugins { get; private set; }
		public List<Plugin> VdmPlugins { get; private set; }

		public InstalledPluginManager()
		{
			PersistencePlugins = new List<Plugin>();
			InterruptionsPlugins = new List<Plugin>();
			VdmPlugins = new List<Plugin>();
		}

		public void InitializePluginContainers()
		{
			_vdmSettings = new LoadedSettings( true, false );
			VdmPlugins = GetVdms();

			try
			{
				_interruptionAggregator = new InterruptionAggregator( App.InterruptionsPluginLibrary );
				InterruptionsPlugins = GetPlugins( _interruptionAggregator, PluginType.Interruptions, App.InterruptionsPluginLibrary );
			}
			catch ( CompositionException exception )
			{
				PluginCompositionFailEvent( exception.RootCauses[ 0 ].Message +
				                            " \n Please download an updated version of this plug-in and restart plug-in manager.", null );
			}

			try
			{
				_persistenceProvider = new PersistenceProvider( App.PersistencePluginLibrary );
				PersistencePlugins = GetPlugins( _persistenceProvider, PluginType.Persistence, App.PersistencePluginLibrary );
			}
			catch ( CompositionException exception )
			{
				PluginCompositionFailEvent( exception.RootCauses[ 0 ].Message +
				                            " \n Please download an updated version of this plug-in and restart plug-in manager.", null );
			}
		}

		List<Plugin> GetPlugins( IInstallablePluginContainer pluginContainer, PluginType type, string pluginLibrary )
		{
			var plugins = new List<Plugin>();
			var dllsPaths = Directory.EnumerateFiles( pluginLibrary, "*.dll" ).ToList();
			pluginContainer.GetPluginInformation().ForEach( pluginInformation =>
			{
				var app = AddIfAbsent( pluginInformation.ProcessName, pluginInformation.CompanyName, plugins );
				switch ( type )
				{
					case PluginType.Interruptions:
						app.Interruptions.Add( CreateConfiguration( pluginInformation, dllsPaths ) );
						break;
					case PluginType.Persistence:
						app.Persistence.Add( CreateConfiguration( pluginInformation, dllsPaths ) );
						break;
				}
			} );
			PluginManagmentHelper.SortByName( ref plugins );
			return plugins;
		}

		Configuration CreateConfiguration( PluginInformation info, IEnumerable<string> dllsPaths )
		{
			var dllPath = dllsPaths.FirstOrDefault( dll => dll.ToLower().Contains( info.ProcessName.ToLower() ) );
			return new Configuration( info.SupportedVersions,
				info.Author, new Version( FileHelper.GetDllVersion( dllPath ) ?? "1.0" ),
				FileHelper.GetLastWriteDate( dllPath ), PluginState.Installed );
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
				var app = AddIfAbsent( vdmCfg.Name, vdmCfg.CompanyName, vdms );
				var supportedVersions = vdmCfg.Version != null ? vdmCfg.Version.Split( ',' ).Select( sv => sv.Trim() ).ToList() : new List<string>();
				app.Vdm.Add( new Configuration( supportedVersions, vdmCfg.Author, new Version( vdmCfg.Version ?? "1.0" ), DateTime.Now.ToShortDateString(), PluginState.Installed ) );
			} );
			PluginManagmentHelper.SortByName( ref vdms );
			return vdms;
		}

		public void InstallPlugin( Plugin plugin, PluginType type )
		{
			CheckIfInitialized();

			var pluginContainer = type == PluginType.Persistence ? _persistenceProvider : _interruptionAggregator as IInstallablePluginContainer;
			try
			{
				pluginContainer.Reload();
			}
			catch ( CompositionException exception )
			{
				PluginCompositionFailEvent( exception.RootCauses[ 0 ].Message +
				                            " \n Please download an updated version of this plug-in and restart plug-in manager.", null );
			}

			var pluginToInstall = pluginContainer.GetInstallablePlugin( plugin.Name, plugin.CompanyName );
			if ( pluginToInstall != null && pluginToInstall.Install() )
			{
				PersistencePlugins = GetPlugins( pluginContainer, PluginType.Persistence, App.PersistencePluginLibrary );
				PluginInstalledEvent( "Plug-in for " + plugin.Name + " was installed correctly.", plugin );
			}
			else
			{
				PluginInstalledEvent( "Plug-in for " + plugin.CompanyName + " was not installed.", plugin );
			}
		}

		Plugin AddIfAbsent( string name, string companyName, ICollection<Plugin> plugins )
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

		void CheckIfInitialized()
		{
			if ( _vdmSettings == null || _persistenceProvider == null || _interruptionAggregator == null )
			{
				throw new Exception( "Installed plug-in manager was not initialized properly." );
			}
		}

		protected override void FreeManagedResources()
		{
			if ( _persistenceProvider != null )
			{
				_persistenceProvider.Dispose();
			}
		}

		protected override void FreeUnmanagedResources()
		{
			// Nothing to do.
		}
	}
}