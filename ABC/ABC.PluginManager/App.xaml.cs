using System.IO;
using System.Windows;
using PluginManager.PluginManagment;
using PluginManager.View.AppOverview;
using PluginManager.ViewModel.PluginsOverview;


namespace PluginManager
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App
	{
		protected override void OnStartup( StartupEventArgs e )
		{
			base.OnStartup( e );

			string pluginsPath = e.Args[ 0 ];
			if ( pluginsPath == null || !Directory.Exists( pluginsPath ) )
			{
				MessageBox.Show( "Please specify plug-in installation directory as a command line parameter." );
				Current.Shutdown();
				return;
			}

			// Get available plug-ins.
			var available = new AvailablePluginTrigger();

			// Get all installed plug-ins.
			var installed = new InstalledPluginTrigger( pluginsPath );

			// Get all applications installed on system.
			var sysInstalled = new ApplicationRegistryBrowse();

			var appOverviewViewModel = new AppOverviewViewModel( available.AvailablePlugins, installed.InstalledPlugins, sysInstalled.InstalledOnSystem );
			var appOverview = new AppOverview { DataContext = appOverviewViewModel };
			appOverview.Show();
		}
	}
}