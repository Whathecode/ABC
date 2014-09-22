
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
	}
}
