
using System.Windows;
using System.Windows.Input;
using PluginManager.Model;
using PluginManager.ViewModel.PluginList;


namespace PluginManager.View.PluginList
{
	/// <summary>
	/// Interaction logic for PluginList.xaml
	/// </summary>
	public partial class PluginList
	{
		public PluginList()
		{
			InitializeComponent();
			ListView.LostFocus += (sender, args) => ListView.SelectedItems.Clear();
		}

		new void PreviewMouseLeftButtonDown( object sender, MouseButtonEventArgs e )
		{
			// Hack, set the selected idem before Install command is executed. 
			var image = (FrameworkElement)sender;
			var conf = (Configuration)image.DataContext;
			var pluginListViewModel = (PluginListViewModel)DataContext;
			pluginListViewModel.SelectedConfigurationItem = conf;

			ListView.Focus();
		}
	}
}
