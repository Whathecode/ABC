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
		readonly List<PluginDetailsViewModel> _installedAndAvailableApps = new List<PluginDetailsViewModel>();
		readonly List<PluginDetailsViewModel> _availableInterruptions = new List<PluginDetailsViewModel>();
		readonly List<PluginDetailsViewModel> _installedInterruptions = new List<PluginDetailsViewModel>();
		readonly List<PluginDetailsViewModel> _installedOnSystem = new List<PluginDetailsViewModel>();

		public AppOverviewViewModel( List<Plugin> available, List<Plugin> installed, List<Plugin> installedOnSystem )
		{
			installed.ForEach( plugin =>
			{
				plugin.Vdm.ForEach( vdm => vdm.State = PluginState.Installed );
				plugin.Persistence.ForEach( persistence => persistence.State = PluginState.Installed );
				plugin.Interruptions.ForEach( interruption => interruption.State = PluginState.Installed );
			} );

			GiveDisplayId( installed );
			installed = MergePlugins( installed );

			available.ForEach( plugin =>
			{
				plugin.Vdm.ForEach( vdm => vdm.State = PluginState.Availible );
				plugin.Persistence.ForEach( persistence => persistence.State = PluginState.Availible );
				plugin.Interruptions.ForEach( interruption => interruption.State = PluginState.Availible );
			} );

			GiveDisplayId( available );
			available = MergePlugins( available );

			// Create two plug-ins collections of application and interruption type.
			available.Where( app => app.Interruptions.Count != 0 ).ForEach( i => _availableInterruptions.Add( new PluginDetailsViewModel( i ) ) );
			available.Where( app => app.Interruptions.Count == 0 ).ForEach( a => _availableApps.Add( new PluginDetailsViewModel( a ) ) );

			// Populate installed applications plug-ins and interruptions.
			installed.Where( app => app.Interruptions.Count != 0 ).ForEach( i => _installedInterruptions.Add( new PluginDetailsViewModel( i ) ) );
			installed.Where( app => app.Interruptions.Count == 0 ).ForEach( a => _installedApps.Add( new PluginDetailsViewModel( a ) ) );

			// Merge available applications with installed to show them together in "all" view.
			var installedAndAvailable = MergePlugins( installed.Where( app => app.Interruptions.Count == 0 ).Concat( available.Where( app => app.Interruptions.Count == 0 ) ) );
			installedAndAvailable.ForEach( a => _installedAndAvailableApps.Add( new PluginDetailsViewModel( a ) ) );

			// By default all application-connected plug-ins are shown on the start screen.
			DisplayedPlugins = new ObservableCollection<PluginDetailsViewModel>( SortByAppName( _installedAndAvailableApps ) );
			State = OverviewState.Applications;

			installedOnSystem.ForEach( installedOnSys => _installedOnSystem.Add( new PluginDetailsViewModel( installedOnSys ) ) );

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
			DisplayedPlugins = new ObservableCollection<PluginDetailsViewModel>( SortByAppName( _installedAndAvailableApps ) );
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

		List<Plugin> MergePlugins( IEnumerable<Plugin> installed )
		{
			var grouped = installed.GroupBy( plugin => plugin.Name.ToLower() ).ToList();

			var merged = new List<Plugin>();
			grouped.ForEach( groupedElement =>
			{
				if ( groupedElement.Count() > 1 )
				{
					var newPlugin = new Plugin( groupedElement.First() );
					foreach ( var plugin in groupedElement )
					{
						newPlugin.Vdm.AddRange( plugin.Vdm );
						newPlugin.Persistence.AddRange( plugin.Persistence );
						newPlugin.Interruptions.AddRange( plugin.Interruptions );
					}
					merged.Add( newPlugin );
				}
				else
				{
					merged.Add( groupedElement.First() );
				}
			} );
			return merged;
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