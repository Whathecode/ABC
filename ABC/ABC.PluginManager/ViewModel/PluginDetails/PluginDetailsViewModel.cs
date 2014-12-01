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

		[NotifyProperty( Binding.Properties.VdmState )]
		public PluginState VdmState { get; private set; }

		[NotifyProperty( Binding.Properties.PersistanceState )]
		public PluginState PersistanceState { get; private set; }

		[NotifyProperty( Binding.Properties.InterruptionsState )]
		public PluginState InterruptionsState { get; private set; }

		public Plugin Plugin { get; private set; }
		public PluginListViewModel VdmList { get; private set; }
		public PluginListViewModel InterruptionsList { get; private set; }
		public PluginListViewModel PersistanceList { get; private set; }

		public PluginDetailsViewModel( Plugin plugin, PluginState state )
			: this( plugin, state, state, state ) {}


		public PluginDetailsViewModel( Plugin plugin, PluginState vdmState , PluginState interruptionsState, PluginState persistenceState)
		{
			Plugin = plugin;
			VdmState = vdmState;
			PersistanceState = persistenceState;
			InterruptionsState = interruptionsState;
			
			// Initialize configurations collections.
			VdmList = new PluginListViewModel( "VDM", plugin.Vdm, this );
			InterruptionsList = new PluginListViewModel( "Interruptions", plugin.Interruptions, this );
			PersistanceList = new PluginListViewModel( "Persistence", plugin.Persistence, this );
			

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