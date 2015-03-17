using System;
using System.IO;
using System.Linq;
using System.Windows;
using ABC.Applications.Persistence;
using System.ComponentModel.Composition;
using ABC.Interruptions;
using ABC.Workspaces.Windows.Settings;
using PluginManager.Common;
using PluginManager.Model;
using PluginManager.PluginManagment;
using PluginManager.View.AppOverview;
using PluginManager.ViewModel.AppOverview;
using PluginManager.ViewModel.PluginOverview;
using Whathecode.System.Extensions;


namespace PluginManager
{
	public partial class App
	{
		public static string PluginManagerDirectory { get; private set; }
		public static string InterruptionsPluginLibrary { get; private set; }
		public static string PersistencePluginLibrary { get; private set; }
		public static string VdmPluginLibrary { get; private set; }

		PluginManifestManager _manifestManager;
		AppOverviewViewModel _appOverviewViewModel;
		AppOverview _appOverview;

		InstalledPluginContainer _persistenceContainer;
		InstalledPluginContainer _interruptionsContainer;
		InstalledPluginContainer _vdmsContainer;

		protected override void OnStartup( StartupEventArgs e )
		{
			base.OnStartup( e );


			PluginManagerDirectory = e.Args.Length > 0 ? e.Args[ 0 ] : null;
			if ( PluginManagerDirectory == null || !Directory.Exists( PluginManagerDirectory ) )
			{
				MessageBox.Show( "Please specify plug-in installation directory as a command line parameter." );
				Current.Shutdown();
				return;
			}

			InterruptionsPluginLibrary = Path.Combine( PluginManagerDirectory, "InterruptionHandlers" );
			PersistencePluginLibrary = Path.Combine( PluginManagerDirectory, "ApplicationPersistence" );
			VdmPluginLibrary = Path.Combine( PluginManagerDirectory, "VdmSettings" );

			_manifestManager = new PluginManifestManager();

			_vdmsContainer = new InstalledPluginContainer( new LoadedSettings( false, false, null, VdmPluginLibrary ));
			_vdmsContainer.PluginInstalledEvent += ( message, guid ) => MessageBox.Show( message );
			try
			{
				_persistenceContainer = new InstalledPluginContainer( new PersistenceProvider( PersistencePluginLibrary ) );
				_persistenceContainer.PluginCompositionFailEvent += ( message, plugin ) => MessageBox.Show( message );
				_persistenceContainer.PluginInstalledEvent += ( message, plugin ) => MessageBox.Show( message );
				_interruptionsContainer = new InstalledPluginContainer( new InterruptionAggregator( InterruptionsPluginLibrary ) );
				_interruptionsContainer.PluginCompositionFailEvent += ( message, plugin ) => MessageBox.Show( message );
				_interruptionsContainer.PluginInstalledEvent += ( message, plugin ) => MessageBox.Show( message );
			}
			catch ( CompositionException exception )
			{
				MessageBox.Show( exception.Message );
			}

			// Check which plug-ins, applications are installed.
			_manifestManager.PluginManifest.Plugin.ForEach( plugin =>
			{
				var container = GetContainer( plugin.Type );
				if ( container == null )
					return;

				// Check if all ABC plug-ins are installed to indicate manager plug-in state.
				var installed = plugin.AbcPlugins.Where( abcPlugin => container.IsInstalled( new Guid( abcPlugin.Guid ) ) ).ToList();
				
				// All ABC plug-ins are installed.
				if ( installed.Count() == plugin.AbcPlugins.Count() )
				{
					plugin.PluginState = PluginState.Installed;
					
					//Check if any of ABC plug-ins is out of date.
					if ( plugin.AbcPlugins.Any( abcplugin => container.CompareVersion( new Guid( abcplugin.Guid ), new Version( abcplugin.Version )) != 0 ) )
					{
						plugin.PluginState = PluginState.Updates;
					}
				}
				// Not all ABC plug-ins are installed.
				else if (installed.Any())
				{
					plugin.PluginState = PluginState.Updates;
				}
				// None of ABC plug-ins are installed.
				else
				{
					plugin.PluginState = PluginState.Availible;
				}
			} );

			_appOverviewViewModel = new AppOverviewViewModel( _manifestManager.PluginManifest );

			_appOverviewViewModel.InstallingPluginEvent += OnInstallingPluginEvent;
			_appOverview = new AppOverview { DataContext = _appOverviewViewModel };
			_appOverview.Show();

			// Dispose MEF container on exit.
			Exit += ( sender, args ) => { if ( _persistenceContainer != null ) _persistenceContainer.Dispose(); };

			// Dispose MEF container on unhandled exception.
			AppDomain.CurrentDomain.UnhandledException += ( s, a ) => { if ( _persistenceContainer != null ) _persistenceContainer.Dispose(); };
		}

		InstalledPluginContainer GetContainer( PluginType pluginType )
		{
			InstalledPluginContainer container = null;
			switch ( pluginType )
			{
				case PluginType.Persistence:
					container = _persistenceContainer;
					break;
				case PluginType.Interruption:
					container = _interruptionsContainer;
					break;
				case PluginType.Vdm:
					container = _vdmsContainer;
					break;
			}
			return container;
		}

		void OnInstallingPluginEvent( PluginViewModel pluginViewModel, Guid abcPluginGuid )
		{
			var container = GetContainer( pluginViewModel.PluginType );
			if (container == null)
				return;
			container.InstallPlugin( pluginViewModel.Guid );
		}
	}
}