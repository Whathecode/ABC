﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using ABC.Applications.Persistence;
using ABC.Common;
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
		public delegate void PluginManagerEventHandler( string message );

		/// <summary>
		///   Event which is triggered when plug-in cannot be loaded.
		/// </summary>
		public event PluginManagerEventHandler PluginCompositionFailEvent;

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
				                            " \n Please download an updated version of this plug-in and restart plug-in manager." );
			}

			try
			{
				_persistenceProvider = new PersistenceProvider( App.PersistencePluginLibrary );
				PersistencePlugins = GetPlugins( _persistenceProvider, PluginType.Persistence, App.PersistencePluginLibrary );
			}
			catch ( CompositionException exception )
			{
				PluginCompositionFailEvent( exception.RootCauses[ 0 ].Message +
				                            " \n Please download an updated version of this plug-in and restart plug-in manager." );
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
			return CreateConfiguration( info.SupportedVersions,
				info.Author, FileHelper.GetDllVersion( dllPath ),
				FileHelper.GetLastWriteDate( dllPath ) );
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
				app.Vdm.Add( CreateConfiguration( supportedVersions, vdmCfg.Name, vdmCfg.Version, DateTime.Now.ToShortDateString() ) );
			} );
			PluginManagmentHelper.SortByName( ref vdms );
			return vdms;
		}

		public bool InstallPlugin( string name, string companyName, PluginType type )
		{
			CheckIfInitialized();
			if ( type == PluginType.Vdm )
			{
				return true;
			}

			var pluginContainer = type == PluginType.Persistence ? _persistenceProvider : _interruptionAggregator as IInstallablePluginContainer;
			pluginContainer.Reload();
			var pluginToInstall = pluginContainer.GetInstallablePlugin( name, companyName );
			return pluginToInstall != null && pluginToInstall.Install();
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