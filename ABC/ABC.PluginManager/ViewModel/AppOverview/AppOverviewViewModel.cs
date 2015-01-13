﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using PluginManager.Common;
using PluginManager.Model;
using PluginManager.PluginManagment;
using PluginManager.ViewModel.PluginDetails;
using PluginManager.ViewModel.PluginList;
using PluginManager.ViewModel.PluginsOverview.Binding;
using Whathecode.System.ComponentModel.NotifyPropertyFactory.Attributes;
using Whathecode.System.Extensions;
using Whathecode.System.Windows.Aspects.ViewModel;
using Whathecode.System.Windows.Input.CommandFactory.Attributes;
using Whathecode.System.Xaml.Behaviors;


namespace PluginManager.ViewModel.PluginsOverview
{
	[ViewModel( typeof( Binding.Properties ), typeof( Commands ) )]
	public class AppOverviewViewModel
	{
		public delegate void PluginOverviewEventHandler( Plugin pluginDetails, PluginListViewModel pluginOverview );

		/// <summary>
		///   Event which is triggered when plug-in being installed.
		/// </summary>
		public event PluginOverviewEventHandler InstallingPluginEvent;

		[NotifyProperty( Binding.Properties.AvailibleApps )]
		public ObservableCollection<PluginOverviewViewModel> DisplayedPlugins { get; set; }

		[NotifyProperty( Binding.Properties.SelectedApplication )]
		public PluginOverviewViewModel SelectedApplication { get; set; }

		[NotifyProperty( Binding.Properties.State )]
		public OverviewState State { get; private set; }

		readonly ObservableCollection<PluginOverviewViewModel> _installedApps = new ObservableCollection<PluginOverviewViewModel>();
		readonly ObservableCollection<PluginOverviewViewModel> _availableApps = new ObservableCollection<PluginOverviewViewModel>();
		readonly ObservableCollection<PluginOverviewViewModel> _installedAndAvailableApps = new ObservableCollection<PluginOverviewViewModel>();
		readonly ObservableCollection<PluginOverviewViewModel> _availableInterruptions = new ObservableCollection<PluginOverviewViewModel>();
		readonly ObservableCollection<PluginOverviewViewModel> _installedInterruptions = new ObservableCollection<PluginOverviewViewModel>();
		readonly ObservableCollection<PluginOverviewViewModel> _installedAndIvailableInterruptions = new ObservableCollection<PluginOverviewViewModel>();
		readonly ObservableCollection<PluginOverviewViewModel> _installedOnSystem = new ObservableCollection<PluginOverviewViewModel>();

		public AppOverviewViewModel( List<Plugin> available, List<Plugin> installed, List<Plugin> installedOnSystem )
		{
			PluginManagmentHelper.GiveId( installed );
			PluginManagmentHelper.GiveId( available );

			available.Concat( installed ).Concat( installedOnSystem ).ForEach( plugin =>
			{
				plugin.CompanyName = plugin.CompanyName ?? "Unknown company";
				plugin.Icon = plugin.Icon ?? new Uri( "pack://application:,,,/View/icons/defaultApp.png" ).AbsolutePath;
			} );

			// Create two plug-ins collections of application and interruption type.
			available.Where( app => app.Interruptions.Any() ).ForEach( i => _availableInterruptions.Add( CreateHookPluginDetails( i ) ) );
			available.Where( app => app.Persistence.Any() || app.Vdm.Any() ).ForEach( a => _availableApps.Add( CreateHookPluginDetails( a ) ) );

			// Populate installed applications plug-ins and interruptions.
			installed.Where( app => app.Interruptions.Any() ).ForEach( i => _installedInterruptions.Add( CreateHookPluginDetails( i ) ) );
			installed.Where( app => app.Persistence.Any() || app.Vdm.Any() ).ForEach( a => _installedApps.Add( CreateHookPluginDetails( a ) ) );

			// Merge available interruptions with installed to show them together in "all" view.
			var installedAndAvailableInterruptions = PluginManagmentHelper.MergePluginsByName(
				installed.Where( app => app.Interruptions.Any() )
					.Concat( available.Where( app => app.Interruptions.Any() ) ) );
			PluginManagmentHelper.GiveId( installedAndAvailableInterruptions );
			PluginManagmentHelper.SortByName( ref installedAndAvailableInterruptions );
			installedAndAvailableInterruptions.ForEach( a => _installedAndIvailableInterruptions.Add( CreateHookPluginDetails( a ) ) );

			// Merge available applications with installed to show them together in "all" view.
			var installedAndAvailable = PluginManagmentHelper.MergePluginsByName(
				installed.Where( app => app.Persistence.Any() || app.Vdm.Any() )
					.Concat( available.Where( app => app.Persistence.Any() || app.Vdm.Any() ) ) );
			PluginManagmentHelper.GiveId( installedAndAvailable );
			PluginManagmentHelper.SortByName( ref installedAndAvailable );
			installedAndAvailable.ForEach( a => _installedAndAvailableApps.Add( CreateHookPluginDetails( a ) ) );

			// By default all application-connected plug-ins are shown on the start screen.
			DisplayedPlugins = _installedAndAvailableApps;
			State = OverviewState.Applications;

			installedOnSystem.ForEach( installedOnSys => _installedOnSystem.Add( CreateHookPluginDetails( installedOnSys ) ) );

			SelectFirst();
		}

		PluginOverviewViewModel CreateHookPluginDetails( Plugin plugin )
		{
			var pluginDeatils = new PluginOverviewViewModel( plugin );
			pluginDeatils.IntallingPluginEvent += ( pluginDetailsInside, pluginList ) => InstallingPluginEvent( SelectedApplication.Plugin, pluginList );
			return pluginDeatils;
		}

		[CommandExecute( Commands.SwitchOverviewState )]
		public void SwitchOverviewState( object commandArg )
		{
			var state = new OverviewState();
			if ( commandArg is OverviewState )
			{
				state = (OverviewState)commandArg;
			}
			else
			{
				var mouseBehaviorData = commandArg as MouseBehaviorData;
				if ( mouseBehaviorData != null && mouseBehaviorData.MouseBehaviorParameter != null )
				{
					state = (OverviewState)mouseBehaviorData.MouseBehaviorParameter;
				}
			}
			switch ( state )
			{
				case OverviewState.AllApplications:
					DisplayedPlugins = _installedAndAvailableApps;
					break;
				case OverviewState.AvailibleApplications:
					DisplayedPlugins = _availableApps;
					break;
				case OverviewState.InstalledApplications:
					DisplayedPlugins = _installedApps;
					break;
				case OverviewState.AllInterruptions:
					DisplayedPlugins = _installedAndIvailableInterruptions;
					break;
				case OverviewState.AvailibleInterruptions:
					DisplayedPlugins = _availableInterruptions;
					break;
				case OverviewState.InstalledInterruptions:
					DisplayedPlugins = _installedInterruptions;
					break;
				case OverviewState.InstalledOnSystem:
					DisplayedPlugins = _installedOnSystem;
					break;
				default:
					DisplayedPlugins = _installedAndAvailableApps;
					break;
			}
			State = state;
			SelectFirst();
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