using System.Collections.Generic;
using System.Linq;
using PluginManager.common;
using PluginManager.Common;
using PluginManager.Model;
using PluginManager.ViewModel.PluginList;
using PluginManager.ViewModel.PluginOverview.Binding;
using Whathecode.System.ComponentModel.NotifyPropertyFactory.Attributes;
using Whathecode.System.Windows.Aspects.ViewModel;
using Whathecode.System.Windows.Input.CommandFactory.Attributes;


namespace PluginManager.ViewModel.PluginOverview
{
	[ViewModel( typeof( Binding.Properties ), typeof( Commands ) )]
	public class PluginOverviewViewModel
	{
		public delegate void PluginEventHandler( PluginOverviewViewModel pluginOverview, PluginListViewModel pluginList );

		/// <summary>
		///   Event which is triggered when plug-in being installed.
		/// </summary>
		public event PluginEventHandler IntallingPluginEvent;

		[NotifyProperty( Binding.Properties.SelectedConfigurationItem )]
		public Configuration SelectedConfigurationItem { get; set; }

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


		public PluginOverviewViewModel( Plugin plugin )
		{
			Plugin = plugin;
			VdmState = VerifyPluginsState( plugin.Vdm );
			PersistanceState = VerifyPluginsState( plugin.Persistence );
			InterruptionsState = VerifyPluginsState( plugin.Interruptions );

			// Initialize configurations collections.
			VdmList = new PluginListViewModel( PluginType.Vdm, plugin.Vdm, this );
			InterruptionsList = new PluginListViewModel( PluginType.Interruptions, plugin.Interruptions, this );
			PersistanceList = new PluginListViewModel( PluginType.Persistence, plugin.Persistence, this );

			// Select first element in ordered configurations collections. 
			if ( plugin.Interruptions.Any() )
			{
				SelectedConfigurationItem = plugin.Interruptions.First();
			}
			else if ( plugin.Vdm.Any() )
			{
				SelectedConfigurationItem = plugin.Vdm.First();
			}
			else if ( plugin.Persistence.Any() )
			{
				SelectedConfigurationItem = plugin.Persistence.First();
			}
		}

		PluginState VerifyPluginsState( IEnumerable<Configuration> pluList )
		{
			if ( pluList.Any( cfg => cfg.State == PluginState.Installed ) )
			{
				return PluginState.Installed;
			}
			return PluginState.Availible;
		}

		[CommandExecute( Commands.CreateNewConfiguartion )]
		public void OpenActivityLibrary()
		{
			// TODO: Implement method what will create new vdm configuration.
			//Process.Start( "notepad.exe", "NewProcessConfiguration.xml" );
		}

		public void InstallPlugin( PluginListViewModel pluginList )
		{
			IntallingPluginEvent( this, pluginList );
		}
	}
}