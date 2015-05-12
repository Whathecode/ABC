using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using PluginManager.Model;
using PluginManager.PluginManagment;
using PluginManager.View.AppOverview;
using PluginManager.ViewModel.AppOverview;


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

		PluginProvider _persistenceContainer;
		PluginProvider _interruptionsContainer;
		PluginProvider _vdmsContainer;

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

			_persistenceContainer = PluginProviderFactory.CreateProvider( PersistencePluginLibrary, PluginType.Persistence, "*.dll" );
						_persistenceContainer.Error += ( sender, args ) => {  };
			var providerDict = new Dictionary<PluginType, PluginProvider>
			{
				{ PluginType.Persistence, _persistenceContainer },
				{ PluginType.Interruption, _interruptionsContainer },
				{ PluginType.Vdm, _vdmsContainer }
			};
			_manifestManager.PluginManifest.ChceckPluginState( providerDict );

			_appOverviewViewModel = new AppOverviewViewModel( _manifestManager.PluginManifest );
			_appOverview = new AppOverview { DataContext = _appOverviewViewModel };
			_appOverview.Show();

			// Dispose MEF container on exit.
			Exit += ( sender, args ) => { if ( _persistenceContainer != null ) _persistenceContainer.Dispose(); };

			// Dispose MEF container on unhandled exception.
			//AppDomain.CurrentDomain.UnhandledException += ( s, a ) => { if ( _persistenceContainer != null ) _persistenceContainer.Dispose(); };
		}
	}
}