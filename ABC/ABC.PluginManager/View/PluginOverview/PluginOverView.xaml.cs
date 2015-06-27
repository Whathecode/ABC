using System.Windows.Controls;
using System.Windows.Input;
using PluginManager.ViewModel.Plugin;
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

			Loaded += ( sender, args ) => PluginsListView.Focus();
			PluginsListView.SelectedIndex = 0;
		}

		void SelectedPluginsChanged( object sender, SelectionChangedEventArgs e )
		{
			// Hack: Since ListView component does not support binding to collections, 
			// we have to update it manually.
			var viewModel = (PluginOverviewViewModel)DataContext;

			// If plug-ins are refreshed from separate thread viewModel can be null (sometimes?).
			if ( viewModel == null )
				return;

			foreach ( var added in e.AddedItems )
			{
				viewModel.SelectedPlugins.Add( (PluginViewModel)added );
			}
			foreach ( var deleted in e.RemovedItems )
			{
				viewModel.SelectedPlugins.Remove( (PluginViewModel)deleted );
			}
		}

		void PluginLeftButtonDown( object sender, MouseButtonEventArgs e )
		{
			PluginsListView.Focus();
		}

		void CursorToHand( object sender, MouseEventArgs e )
		{
			if ( Cursor != Cursors.Wait )
				Mouse.OverrideCursor = Cursors.Hand;
		}

		void CursorToArrow( object sender, MouseEventArgs e )
		{
			if ( Cursor != Cursors.Wait )
				Mouse.OverrideCursor = Cursors.Arrow;
		}
	}
}