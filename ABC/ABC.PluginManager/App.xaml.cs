using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
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

		/// <summary>
		/// Process to run after application is closed.
		/// </summary>
		public string PostProcessPath { get; private set; }

		PluginManifestManager _manifestManager;
		AppOverviewViewModel _appOverviewViewModel;
		AppOverview _appOverview;


		protected override void OnStartup( StartupEventArgs e )
		{
			base.OnStartup( e );

			PluginManagerDirectory = e.Args.ElementAtOrDefault( 0 );
			if ( string.IsNullOrEmpty( PluginManagerDirectory ) || !Directory.Exists( PluginManagerDirectory ) )
			{
				MessageBox.Show( "Plug-in installation directory has to be passed as a command line parameter." );
				Current.Shutdown();

				// In rare case shutdown does not work causing application crash?
				Environment.Exit( 0 );
			}

			PostProcessPath = e.Args.ElementAtOrDefault( 1 );

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

			// Dispose MEF container on exit and any unhandled exception.
			Exit += ( sender, args ) => pluginManagerController.Dispose();
			AppDomain.CurrentDomain.UnhandledException += ( s, a ) => pluginManagerController.Dispose();
		}

		void OnExit( object sender, ExitEventArgs e )
		{
			if (string.IsNullOrEmpty( PostProcessPath ))
				return;

			Mouse.OverrideCursor = Cursors.Wait;
			Process.Start( PostProcessPath );
		}
	}
}