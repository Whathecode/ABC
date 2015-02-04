using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using ABC.Applications.Chrome;
using ABC.Applications.Explorer;
using ABC.Applications.Notepad;
using ABC.Applications.Persistence;
using Whathecode.System.Windows;


namespace ABC.Debug
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{
		readonly CollectionPersistenceProvider _persistenceProvider = new CollectionPersistenceProvider();
		public ObservableCollection<AbstractApplicationPersistence> PersistenceProviders { get; private set; }

		readonly List<PersistedApplication> _persistedStates = new List<PersistedApplication>();


		public MainWindow()
		{
			Closing += ( sender, args ) => _persistenceProvider.Dispose();

			// Load application persistence plugins.
			PersistenceProviders = new ObservableCollection<AbstractApplicationPersistence>
			{
				new NotepadPersistence(),
				new ExplorerPersistence(),
				new ChromePersistence()
			};

			// Since we are not using MVVM, make sure to initialize components after all data is prepared.
			InitializeComponent();

			ResumeButton.IsEnabled = false;
		}


		void OnSuspend( object sender, RoutedEventArgs e )
		{
			// Suspend all windows of selected persistence provider.
			var persistor = (AbstractApplicationPersistence)PersistenceProviderList.SelectedItem;
			_persistenceProvider.PersistenceProviders.Add( persistor );
			List<PersistedApplication> data = _persistenceProvider.Suspend( WindowManager.GetWindows().Select( w => new Window( w ) ).ToList() );
			ResumeButton.IsEnabled = true;
			_persistedStates.AddRange( data );
			_persistenceProvider.PersistenceProviders.Remove( persistor );
		}

		void OnResume( object sender, RoutedEventArgs e )
		{
			_persistenceProvider.PersistenceProviders.AddRange( PersistenceProviders );

			// Resume all persisted state.
			_persistenceProvider.Resume( _persistedStates );

			ResumeButton.IsEnabled = false;
			_persistedStates.Clear();
			_persistenceProvider.PersistenceProviders.Clear();
		}
	}
}
