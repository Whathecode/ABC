using System;
using System.IO;
using System.Linq;
using System.Windows;
using PluginManager.Common;
using PluginManager.Model;
using PluginManager.PluginManagment;
using PluginManager.View.AppOverview;
using PluginManager.ViewModel.AppOverview;
using PluginManager.ViewModel.PluginList;


namespace PluginManager
{
	public partial class App
	{
		public static string PluginManagerDirectory { get; private set; }
		public static string InterruptionsPluginLibrary { get; private set; }
		public static string PersistencePluginLibrary { get; private set; }
		public static string VdmPluginLibrary { get; private set; }

		InstalledPluginManager _installedManager;
		AvailablePluginManager _availableManager;
		AppOverviewViewModel _appOverviewViewModel;
		ApplicationRegistryBrowse _sysInstalled;
		AppOverview _appOverview;

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

			// Get all available plug-ins.
			_availableManager = new AvailablePluginManager();


			// Get all installed plug-ins.
			_installedManager = new InstalledPluginManager();
			_installedManager.PluginCompositionFailEvent += (message, plugin) => MessageBox.Show( message );
			_installedManager.PluginInstalledEvent += ( message, plugin ) =>
			{
				MessageBox.Show( message );
				_appOverviewViewModel.Populate( _availableManager.AvailablePlugins,  PluginManagmentHelper.MergePluginsByName(_installedManager.PersistencePlugins
					.Concat( _installedManager.InterruptionsPlugins ).Concat( _installedManager.VdmPlugins )
					.ToList()), _sysInstalled.InstalledOnSystem );
			};
			_installedManager.InitializePluginContainers();

			// Get all applications installed on system.
			_sysInstalled = new ApplicationRegistryBrowse();

			_appOverviewViewModel = new AppOverviewViewModel(
				_availableManager.AvailablePlugins, PluginManagmentHelper.MergePluginsByName( _installedManager.PersistencePlugins
					.Concat( _installedManager.InterruptionsPlugins ).Concat( _installedManager.VdmPlugins )
					.ToList()), _sysInstalled.InstalledOnSystem );

			_appOverviewViewModel.InstallingPluginEvent += OnInstallingPluginEvent;
			_appOverview = new AppOverview { DataContext = _appOverviewViewModel };
			_appOverview.Show();

			// Dispose MEF container on exit.
			Exit += ( sender, args ) => _installedManager.Dispose();

			// Dispose MEF container on unhandled exception.
			AppDomain.CurrentDomain.UnhandledException += ( s, a ) => _installedManager.Dispose();
		}

		void OnInstallingPluginEvent( Plugin plugin, PluginListViewModel pluginOverview )
		{
			var downloader = new PluginDownloader( pluginOverview.SelectedConfigurationItem, pluginOverview.PluginType );

			var tempConf = pluginOverview.SelectedConfigurationItem;
			var targetConfiguration = new Configuration(tempConf.SupportedVersions, tempConf.Author, tempConf.Version, tempConf.TimeStamp, PluginState.Installed);
			plugin.Vdm.Clear();
				plugin.Interruptions.Clear();
				plugin.Persistence.Clear();
				var configurations = pluginOverview.PluginType == PluginType.Interruptions
					? plugin.Interruptions
					: pluginOverview.PluginType == PluginType.Persistence
						? plugin.Persistence
						: plugin.Vdm;
				configurations.Add( targetConfiguration );
			
			downloader.PluginDownloadedEvent += ( configuration, completedEventArgs ) =>
			{
				if ( completedEventArgs.Cancelled )
				{
					return;
				}

				_installedManager.InstallPlugin( plugin, pluginOverview.PluginType );
			};
			downloader.DownloadAsync();
		}
	}
}