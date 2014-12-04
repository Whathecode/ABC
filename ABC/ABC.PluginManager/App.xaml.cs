using System.IO;
using System.Windows;
using PluginManager.PluginManagment;
using PluginManager.View.AppOverview;
using PluginManager.ViewModel.PluginsOverview;


namespace PluginManager
{

	public partial class App
	{
		public static string PluginManagerDirectory { get; private set; }
		public static string InterruptionsPluginLibrary { get; private set; }
		public static string PersistencePluginLibrary { get; private set; }
		public static string VdmPluginLibrary { get; private set; }


		protected override void OnStartup( StartupEventArgs e )
		{
			base.OnStartup( e );

			PluginManagerDirectory = e.Args[ 0 ];
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
			var available = new AvailablePluginTrigger();

			// Get all installed plug-ins.
			var installed = new InstalledPluginTrigger();

			// Get all applications installed on system.
			var sysInstalled = new ApplicationRegistryBrowse();

			var appOverviewViewModel = new AppOverviewViewModel( available.AvailablePlugins, installed.InstalledPlugins, sysInstalled.InstalledOnSystem );
			var appOverview = new AppOverview { DataContext = appOverviewViewModel };
			appOverview.Show();
		}
	}
}