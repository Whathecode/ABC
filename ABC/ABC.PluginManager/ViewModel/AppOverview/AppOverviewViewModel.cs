using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
			CurrentPlugins = new PluginOverviewViewModel( _applicationPlugins[ newApp ] );
			CurrentPlugins.ConfiguringPluginEvent += ( sender, args ) => ConfiguringPluginEvent( sender, args );
		}

		[NotifyProperty( Binding.Properties.Filters )]
		public List<string> Filters { get; private set; }

		[NotifyProperty( Binding.Properties.SelectedFilter )]
		public string SelectedFilter { get; private set; }

		[NotifyProperty( Binding.Properties.CurrentPlugins )]
		public PluginOverviewViewModel CurrentPlugins { get; private set; }

		[NotifyProperty( Binding.Properties.Applications )]
		public ObservableCollection<ApplicationViewModel> Applications { get; set; }

		[NotifyPropertyChanged( Binding.Properties.SelectedFilter )]
		public void OnSelectedFilterChanged( string oldFilter, string newFilter )
		{
			Populate( _pluginManifest );
		}

		// Used to cache plug-ins for applications.
		readonly Dictionary<ApplicationViewModel, List<PluginManifestPlugin>> _applicationPlugins;
		
		readonly PluginManifest _pluginManifest;
		Guid _lastSelectedAppGuid;

		public AppOverviewViewModel( PluginManifest pluginManifest )
		{
			_pluginManifest = pluginManifest;

			Filters = Enum.GetNames( typeof( PluginState ) ).ToList();
			Filters.Insert( 0, "All" );

			_applicationPlugins = new Dictionary<ApplicationViewModel, List<PluginManifestPlugin>>();

			// Set the filter and show applications + plug-ins.
			SelectedFilter = Filters.First();
		}

		public void Populate( PluginManifest pluginManifest )
		{
			Mouse.OverrideCursor = Cursors.Wait;

			PluginState state;
			if ( Enum.TryParse( SelectedFilter, out state ) )
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
			_applicationPlugins.Clear();
			CurrentPlugins = null;

			_pluginManifest.Application.ForEach( manifestApplication =>
			{
				var appPlugins = _pluginManifest.GivePlugins( new Guid( manifestApplication.Guid ), state ).ToList();
				var application = new ApplicationViewModel( manifestApplication, appPlugins );
				if ( appPlugins.Count > 0 )
				{
					_applicationPlugins.Add( application, appPlugins );
				}
			} );

			Applications = new ObservableCollection<ApplicationViewModel>( _applicationPlugins.Keys.OrderBy( model => model.Name ) );

			// Focus an application that was selected in a previous view.
			if ( _applicationPlugins.Any() )
			{
				var lastSelectedApp = _applicationPlugins.Keys.FirstOrDefault( key => key.Guid == _lastSelectedAppGuid );
				SelectedApplication = lastSelectedApp ?? _applicationPlugins.First().Key;
			}
		}

		[CommandExecute( Commands.SelectApplication )]
		public void SelectApplication()
		{
			CurrentPlugins = new PluginOverviewViewModel( _applicationPlugins[ SelectedApplication ].ToList() );
		}
	}
}