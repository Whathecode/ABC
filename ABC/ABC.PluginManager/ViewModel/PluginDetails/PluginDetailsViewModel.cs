using System.Linq;
using PluginManager.common;
using PluginManager.Model;
using PluginManager.ViewModel.PluginDetails.Binding;
using PluginManager.ViewModel.PluginList;
using Whathecode.System.ComponentModel.NotifyPropertyFactory.Attributes;
using Whathecode.System.Windows.Aspects.ViewModel;
using Whathecode.System.Windows.Input.CommandFactory.Attributes;


namespace PluginManager.ViewModel.PluginDetails
{
	[ViewModel( typeof( Binding.Properties ), typeof( Commands ) )]
	public class PluginDetailsViewModel
	{
		[NotifyProperty( Binding.Properties.SelectedConfigurationItem )]
		public Configuration SelectedConfigurationItem { get; set; }

		[NotifyProperty( Binding.Properties.State )]
		public PluginState State { get; private set; }

		public Plugin Plugin { get; set; }
		public PluginListViewModel VdmListViewModel { get; private set; }
		public PluginListViewModel InterruptionsListViewModel { get; private set; }
		public PluginListViewModel PersistanceListViewModel { get; private set; }


		public PluginDetailsViewModel( Plugin plugin, PluginState state )
		{
			Plugin = plugin;
			State = state;

			// Initialize confutations collections.
			if ( plugin.Vdm != null )
			{
				VdmListViewModel = new PluginListViewModel( "VDM", plugin.Vdm, this );
			}
			if ( plugin.Interruptions != null )
			{
				InterruptionsListViewModel = new PluginListViewModel( "Interruptions", plugin.Interruptions, this );
			}
			if ( plugin.Persistence != null )
			{
				PersistanceListViewModel = new PluginListViewModel( "Persistence", plugin.Persistence, this );
			}

			// Select first element in ordered configurations collections. 
			if ( plugin.Interruptions != null && plugin.Interruptions.Any() )
			{
				SelectedConfigurationItem = plugin.Interruptions.First();
			}
			else if ( plugin.Vdm != null && plugin.Vdm.Any() )
			{
				SelectedConfigurationItem = plugin.Vdm.First();
			}
			else if ( plugin.Persistence != null && plugin.Persistence.Any() )
			{
				SelectedConfigurationItem = plugin.Persistence.First();
			}
		}

		[CommandExecute( Commands.CreateNewConfiguartion )]
		public void OpenActivityLibrary()
		{
			// TODO: Implement method what will create new vdm configuration.
			//Process.Start( "notepad.exe", "NewProcessConfiguration.xml" );
		}
	}
}