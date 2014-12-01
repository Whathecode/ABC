using System;
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
			// By default all application connected plug-ins are shown on a fist screen.
			State = OverviewState.Applications;

			DisplayedPlugins = new ObservableCollection<PluginDetailsViewModel>();

			// Create two plug-ins collections of application and interruption type.
			GiveDisplayId( available );
			foreach ( var plugin in available )
			{
				if ( plugin.Interruptions.Count == 0 )
				{
					_availableApps.Add( new PluginDetailsViewModel( plugin, PluginState.Availible ) );
				}
				else
				{
					_availableInterruptions.Add( new PluginDetailsViewModel( plugin, PluginState.Availible ) );
				}
			}

			GiveDisplayId( installed );

			// Populate installed applications plug-ins and interruptions.
			installed.Where( app => app.Interruptions.Count != 0 ).ForEach( i => _installedInterruptions.Add( new PluginDetailsViewModel( i, PluginState.Installed ) ) );
			installed.Where( app => app.Interruptions.Count == 0 ).ForEach( a => _installedApps.Add( new PluginDetailsViewModel( a, PluginState.Installed ) ) );

			// Merge available application with installed to show them together in "all" view.
			_availableApps.ForEach( availableApp =>
			{
				PluginDetailsViewModel pluginDetails = null;
				var vdmList = new List<Configuration>();
				var persistenceList = new List<Configuration>();
				_installedApps.ForEach( installedApp =>
				{
					if ( availableApp.Plugin.Name.Equals( installedApp.Plugin.Name, StringComparison.CurrentCultureIgnoreCase ) )
					{
						vdmList = availableApp.VdmList.PluginList.Concat( installedApp.VdmList.PluginList ).ToList();
						persistenceList = availableApp.PersistanceList.PluginList.Concat( installedApp.PersistanceList.PluginList ).ToList();
					}
					var plugin = new Plugin
					{
						Icon = availableApp.Plugin.Icon,
						Name = availableApp.Plugin.Name,
						CompanyName = availableApp.Plugin.CompanyName,
						Persistence = persistenceList,
						Vdm = vdmList
					};
					pluginDetails = new PluginDetailsViewModel( plugin, PluginState.Installed, PluginState.Installed, PluginState.Availible );
				} );
				_installedAndAvailableApps.Add( pluginDetails ?? availableApp );
			} );
			_installedAndAvailableApps = SortByAppName( _installedAndAvailableApps.Union( _installedApps, new PluginComparer() ) ).ToList();

			// By default all application connected plug-ins are shown on the start screen.
			DisplayedPlugins = new ObservableCollection<PluginDetailsViewModel>( _installedAndAvailableApps );

			installedOnSystem.ForEach( installedOnSys => _installedOnSystem.Add( new PluginDetailsViewModel( installedOnSys, PluginState.Availible ) ) );

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
			DisplayedPlugins = new ObservableCollection<PluginDetailsViewModel>( _installedAndAvailableApps );
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