using System.Windows;
using ABC.Windows.Desktop.Monitor.View;
using ABC.Windows.Desktop.Monitor.ViewModel;


namespace ABC.Windows.Desktop.Monitor
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

			new MainWindow
			{
				DataContext = _viewModel
			}.Show();
		}
	}
}
