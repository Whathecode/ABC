using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using PluginManager.common;
using PluginManager.Model;
using PluginManager.ViewModel.PluginDetails;
using PluginManager.ViewModel.PluginsOverview.Binding;
using Whathecode.System.ComponentModel.NotifyPropertyFactory.Attributes;
using Whathecode.System.Extensions;
using Whathecode.System.Windows.Aspects.ViewModel;
using Whathecode.System.Windows.Input.CommandFactory.Attributes;


namespace PluginManager.ViewModel.PluginsOverview
{
	[ViewModel( typeof( Binding.Properties ), typeof( Commands ) )]
	public class AppOverviewViewModel
	{
		[NotifyProperty( Binding.Properties.AvailibleApps )]
		public ObservableCollection<PluginDetailsViewModel> DisplayedPlugins { get; set; }

		[NotifyProperty( Binding.Properties.SelectedApplication )]
		public PluginDetailsViewModel SelectedApplication { get; set; }

		[NotifyProperty( Binding.Properties.State )]
		public OverviewState State { get; private set; }

		readonly List<PluginDetailsViewModel> _installedApps = new List<PluginDetailsViewModel>();
		readonly List<PluginDetailsViewModel> _availableApps = new List<PluginDetailsViewModel>();
		readonly List<PluginDetailsViewModel> _availableInterruptions = new List<PluginDetailsViewModel>();
		readonly List<PluginDetailsViewModel> _installedInterruptions = new List<PluginDetailsViewModel>();
		readonly List<PluginDetailsViewModel> _installedOnSystem = new List<PluginDetailsViewModel>();

		public AppOverviewViewModel( List<Plugin> available, List<Plugin> installed, List<Plugin> installedOnSystem )
		{
			// By default all application connected plug-ins are shown on a fist screen.
			State = OverviewState.Applications;

			DisplayedPlugins = new ObservableCollection<PluginDetailsViewModel>();

			// Create two plug-ins collections of application and interruption type.
			GiveDisplayId( available );
			foreach ( var plugin in available )
			{
				if ( plugin.Interruptions.Count == 0 )
				{
					_availableApps.Add( new PluginDetailsViewModel( plugin, PluginFilter.Availible ) );
				}
				else
				{
					_availableInterruptions.Add( new PluginDetailsViewModel( plugin, PluginFilter.Availible ) );
				}
			}

			GiveDisplayId( installed );

			// Populate installed applications plug-ins and interruptions.
			installed.Where( app => app.Interruptions.Count != 0 ).ForEach( i => _installedInterruptions.Add( new PluginDetailsViewModel( i, PluginFilter.Installed ) ) );
			installed.Where( app => app.Interruptions.Count == 0 ).ForEach( a => _installedApps.Add( new PluginDetailsViewModel( a, PluginFilter.Installed ) ) );

			// By default all application connected plug-ins are shown on the start screen.
			DisplayedPlugins = new ObservableCollection<PluginDetailsViewModel>( SortByAppName( _installedApps.Concat( _availableApps ) ) );

			installedOnSystem.ForEach( installedOnSys => _installedOnSystem.Add( new PluginDetailsViewModel( installedOnSys, PluginFilter.Availible ) ) );

			SelectFirst();
		}

		[CommandExecute( Commands.ShowAvailableApps )]
		public void ShowAvailibleApps()
		{
			ChangeState( OverviewState.Applications );
			DisplayedPlugins = new ObservableCollection<PluginDetailsViewModel>( SortByAppName( _availableApps ) );
			SelectFirst();
		}

		[CommandExecute( Commands.ShowAvailableInterruptions )]
		public void ShowAvailableInterruptions()
		{
			ChangeState( OverviewState.Interruptions );
			DisplayedPlugins = new ObservableCollection<PluginDetailsViewModel>( SortByAppName( _availableInterruptions ) );
			SelectFirst();
		}

		[CommandExecute( Commands.ShowInstalledApps )]
		public void ShowInstalledApps()
		{
			ChangeState( OverviewState.Applications );
			DisplayedPlugins = new ObservableCollection<PluginDetailsViewModel>( SortByAppName( _installedApps ) );
			SelectFirst();
		}


		[CommandExecute( Commands.ShowInstalledInterruptions )]
		public void ShowInstallednterruptions()
		{
			ChangeState( OverviewState.Interruptions );
			DisplayedPlugins = new ObservableCollection<PluginDetailsViewModel>( SortByAppName( _installedInterruptions ) );
			SelectFirst();
		}

		[CommandExecute( Commands.ShowAllApplications )]
		public void ShowAllApplications()
		{
			ChangeState( OverviewState.Applications );
			DisplayedPlugins = new ObservableCollection<PluginDetailsViewModel>( SortByAppName( _installedApps.Concat( _availableApps ) ) );
			SelectFirst();
		}

		[CommandExecute( Commands.ShowAllInterruptions )]
		public void ShowAllInterruptions()
		{
			ChangeState( OverviewState.Interruptions );
			DisplayedPlugins = new ObservableCollection<PluginDetailsViewModel>( SortByAppName( _installedInterruptions.Concat( _availableInterruptions ) ) );
			SelectFirst();
		}

		[CommandExecute( Commands.ShowInstalledOnSystem )]
		public void ShowInstalledOnSystem()
		{
			ChangeState( OverviewState.Applications );
			DisplayedPlugins = new ObservableCollection<PluginDetailsViewModel>( SortByAppName( _installedOnSystem ) );
			SelectFirst();
		}

		static IEnumerable<PluginDetailsViewModel> SortByAppName( IEnumerable<PluginDetailsViewModel> listToSort )
		{
			return listToSort.OrderBy( element => element.Plugin.Name );
		}

		void ChangeState( OverviewState state )
		{
			State = state;
		}

		/// <summary>
		/// Used to make list view selection work properly.
		/// </summary>
		static void GiveDisplayId( IEnumerable<Plugin> plugins )
		{
			var pluginId = 1;
			plugins.ForEach( plugin =>
			{
				var concatList = plugin.Interruptions.Concat( plugin.Persistence ).Concat( plugin.Vdm );
				var configurationId = 1;
				foreach ( var configuration in concatList )
				{
					configuration.Id = configurationId++;
				}
				plugin.Id = pluginId++;
			} );
		}

		/// <summary>
		/// Selects the first box in the list view.
		/// </summary>
		void SelectFirst()
		{
			if ( DisplayedPlugins.Count > 0 )
			{
				SelectedApplication = DisplayedPlugins.First();
			}
		}
	}
}