using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Windows.Input;
using PluginManager.Common;
using PluginManager.Model;
using PluginManager.ViewModel.AppOverview.Binding;
using PluginManager.ViewModel.Plugin;
using PluginManager.ViewModel.PluginOverview;
using Whathecode.System.ComponentModel.NotifyPropertyFactory.Attributes;
using Whathecode.System.Extensions;
using Whathecode.System.Windows.Aspects.ViewModel;
using Whathecode.System.Windows.Input.CommandFactory.Attributes;


namespace PluginManager.ViewModel.AppOverview
{
	[ViewModel( typeof( Binding.Properties ), typeof( Commands ) )]
	public class AppOverviewViewModel
	{
		public delegate void PluginEventHandler( PluginViewModel pluginViewModel, EventArgs args );

		/// <summary>
		///   Event which is triggered when plug-in being configured.
		/// </summary>
		public event PluginEventHandler ConfiguringPluginEvent;


		[NotifyProperty( Binding.Properties.SelectedApplication )]
		public ApplicationViewModel SelectedApplication { get; set; }

		[NotifyPropertyChanged( Binding.Properties.SelectedApplication )]
		public void OnSelectedApplicationChanged( ApplicationViewModel oldApp, ApplicationViewModel newApp )
		{
			_lastSelectedAppGuid = newApp.Guid;
			CurrentPlugins = new PluginOverviewViewModel( ApplicationPlugins[ newApp ].ToList() );
			CurrentPlugins.ConfiguringPluginEvent += ( sender, args ) => ConfiguringPluginEvent( sender, args );
		}

		[NotifyProperty( Binding.Properties.Filters )]
		public List<string> Filters { get; private set; }

		[NotifyProperty( Binding.Properties.SelectedFilter )]
		public string SelectedFilter { get; private set; }

		[NotifyProperty( Binding.Properties.CurrentPlugins )]
		public PluginOverviewViewModel CurrentPlugins { get; private set; }

		[NotifyProperty( Binding.Properties.ApplicationPlugins )]
		public ObservableConcurrentDictionary<ApplicationViewModel, List<PluginManifestPlugin>> ApplicationPlugins { get; set; }

		[NotifyPropertyChanged( Binding.Properties.SelectedFilter )]
		public void OnSelectedFilterChanged( string oldFilter, string newFilter )
		{
			Populate( _pluginManifest, newFilter );
		}

		readonly PluginManifest _pluginManifest;

		Guid _lastSelectedAppGuid;


		public AppOverviewViewModel( PluginManifest pluginManifest )
		{
			_pluginManifest = pluginManifest;

			Filters = Enum.GetNames( typeof( PluginState ) ).ToList();
			Filters.Insert( 0, "All" );
			
			// Set the filter and show applications + plug-ins.
			SelectedFilter = Filters.First();
		}

		public void Populate( PluginManifest pluginManifest, string filter = "" )
		{
			Mouse.OverrideCursor = Cursors.Wait;

			PluginState state;
			if ( Enum.TryParse( filter, out state ) )
			{
				FilterApps( state );
			}
			else
			{
				FilterApps();
			}

			Mouse.OverrideCursor = Cursors.Arrow;
		}

		// If state is not provided no filter is used.
		void FilterApps( PluginState state = 0 )
		{
			ApplicationPlugins = new ObservableConcurrentDictionary<ApplicationViewModel, List<PluginManifestPlugin>>();
			CurrentPlugins = null;

			_pluginManifest.Application.ForEach( manifestApplication =>
			{
				var appPlugins = _pluginManifest.GivePlugins( new Guid( manifestApplication.Guid ), state ).ToList();
				var application = new ApplicationViewModel( manifestApplication, appPlugins );
				ApplicationPlugins.Add( application, appPlugins );
			} );

			// Delete all applications that contain no plug-ins.
			ApplicationPlugins.Where( applicationPlugin => applicationPlugin.Value.Count == 0 )
				.ForEach( toRemove => ApplicationPlugins.Remove( toRemove.Key ) );

			if ( ApplicationPlugins.Any() )
			{
				var lastSelectedApp = ApplicationPlugins.Keys.FirstOrDefault( key => key.Guid == _lastSelectedAppGuid );
				SelectedApplication =  lastSelectedApp ?? ApplicationPlugins.First().Key;
			}
		}

		[CommandExecute( Commands.SelectApplication )]
		public void SelectApplication()
		{
			CurrentPlugins = new PluginOverviewViewModel( ApplicationPlugins[ SelectedApplication ].ToList() );
		}
	}
}