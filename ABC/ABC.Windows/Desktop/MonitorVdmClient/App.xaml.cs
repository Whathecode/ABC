using System.Windows;
using MonitorVdmClient.View;
using MonitorVdmClient.ViewModel;

namespace MonitorVdmClient
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App
	{
		MainViewModel _viewModel;


		protected override void OnStartup( StartupEventArgs e )
		{
			_viewModel = new MainViewModel();

			new MainWindow()
			{
				DataContext = _viewModel
			}.Show();
		}
	}
}
