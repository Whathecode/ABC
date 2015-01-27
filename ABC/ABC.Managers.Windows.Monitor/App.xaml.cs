using System.Windows;
using ABC.Managers.Windows.Monitor.View;
using ABC.Managers.Windows.Monitor.ViewModel;


namespace ABC.Managers.Windows.Monitor
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
