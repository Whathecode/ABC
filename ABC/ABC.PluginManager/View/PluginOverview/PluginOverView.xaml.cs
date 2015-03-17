using System.Windows;
using System.Windows.Input;
using PluginManager.ViewModel.PluginOverview;


namespace PluginManager.View.PluginOverview
{
	/// <summary>
	/// Interaction logic for PluginList.xaml
	/// </summary>
	public partial class PluginOverview
	{
		public PluginOverview()
		{
			InitializeComponent();
			ListView.LostFocus += ( sender, args ) => ListView.SelectedItems.Clear();
		}

		new void PreviewMouseLeftButtonDown( object sender, MouseButtonEventArgs e )
		{
			// Hack, set the selected idem before Install command is executed. 
			var image = (FrameworkElement)sender;
			var pluginViewModel = (PluginViewModel)image.DataContext;
			var pluginOverviewViewModel = (PluginOverviewViewModel)DataContext;
			pluginOverviewViewModel.SelectedPlugin = pluginViewModel;

			ListView.Focus();
		}
	}
}