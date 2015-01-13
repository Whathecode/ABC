using System;
using System.IO;
using System.Linq;
using System.Windows;
using PluginManager.Model;
using PluginManager.PluginManagment;
using PluginManager.View.AppOverview;
using PluginManager.ViewModel.PluginList;
using PluginManager.ViewModel.PluginsOverview;


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

			// Get available plug-ins.
			_availableManager = new AvailablePluginManager();

			// Get all installed plug-ins.
			_installedManager = new InstalledPluginManager();

			// Get all applications installed on system.
			var sysInstalled = new ApplicationRegistryBrowse();

			var appOverviewViewModel = new AppOverviewViewModel(
				_availableManager.AvailablePlugins, _installedManager.PersistencePlugins
				.Concat(_installedManager.InterruptionsPlugins ).Concat( _installedManager.VdmPlugins )
				.ToList(), sysInstalled.InstalledOnSystem );
			
		

			appOverviewViewModel.InstallingPluginEvent += OnInstallingPluginEvent;
			var appOverview = new AppOverview { DataContext = appOverviewViewModel };
			appOverview.Show();

			// Dispose MEF container on exit.
			Exit += ( sender, args ) => _installedManager.Dispose();

			// Dispose MEF container on unhandled exception.
			AppDomain.CurrentDomain.UnhandledException += ( s, a ) =>  _installedManager.Dispose();
		}

		void OnInstallingPluginEvent( Plugin plugin, PluginListViewModel pluginOverview )
		{
			var installer = new PluginDownloader( pluginOverview.SelectedConfigurationItem, pluginOverview.PluginType );
			installer.PluginDownloadedEvent += ( configuration, completedEventArgs ) =>
			{
				if ( !completedEventArgs.Cancelled )
				{
					_installedManager.InstallPlugin( plugin.Name, plugin.CompanyName, pluginOverview.PluginType );
				}
			};
			installer.DownloadAsync();
		}
	}
}