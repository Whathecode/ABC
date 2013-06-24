using System.ComponentModel;
using System.Windows.Controls;
using MonitorVdmClient.ViewModel;
using System.Linq;


namespace MonitorVdmClient.View
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{
		public MainWindow()
		{
			InitializeComponent();

			// Select first virtual desktop tab when data is refreshed.
			TypeDescriptor.GetProperties( VirtualDesktopTabs )[ "ItemsSource" ].AddValueChanged( VirtualDesktopTabs, (s, e) => VirtualDesktopTabs.SelectedIndex = 0 ); 
		}


		void OnDesktopWindowsSelectionChanged( object sender, SelectionChangedEventArgs e )
		{
			var windowList = (ListView)sender;
			var viewModel = windowList.DataContext as VirtualDesktopViewModel;
			if ( viewModel == null )
			{
				return;
			}

			viewModel.SelectedWindows.Clear();
			viewModel.SelectedWindows.AddRange( windowList.SelectedItems.Cast<WindowViewModel>() );
		}
	}
}
