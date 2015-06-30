using System;
using System.IO;
using System.Windows;
using PluginManager.PluginManagment;
using PluginManager.View.AppOverview;
using PluginManager.ViewModel.AppOverview;


namespace PluginManager
{
	// Cross-domain communication requires Serializable tag.
	[Serializable]
	public partial class App
	{
		public static string PluginManagerDirectory { get; private set; }
		public static string InterruptionsPluginLibrary { get; private set; }
		public static string PersistencePluginLibrary { get; private set; }
		public static string VdmPluginLibrary { get; private set; }

		PluginManifestManager _manifestManager;
		AppOverviewViewModel _appOverviewViewModel;
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

			_manifestManager = new PluginManifestManager();

			var pluginManagerController = new PluginManagerController();
			pluginManagerController.RefreshingEvent += ( sender, args ) =>
			{
				_manifestManager.PluginManifest.ChceckPluginState( pluginManagerController.ProviderDictionary );
				Current.Dispatcher.Invoke( () => _appOverviewViewModel.Populate( _manifestManager.PluginManifest ) );
			};

			_manifestManager.PluginManifest.ChceckPluginState( pluginManagerController.ProviderDictionary );

			_appOverviewViewModel = new AppOverviewViewModel( _manifestManager.PluginManifest );
			_appOverviewViewModel.ConfiguringPluginEvent +=
				( model, args ) => pluginManagerController.Configure( model.Guid, model.PluginType, model.PluginPath );
			_appOverview = new AppOverview { DataContext = _appOverviewViewModel };
			_appOverview.Show();

			// Dispose MEF container on exit.
			Exit += ( sender, args ) => pluginManagerController.Dispose();

			// Dispose MEF container on unhandled exception.
			AppDomain.CurrentDomain.UnhandledException += ( s, a ) => pluginManagerController.Dispose();
		}
	}
}