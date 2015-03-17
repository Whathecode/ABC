using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using PluginManager.Common;
using PluginManager.Model;
using PluginManager.ViewModel.AppOverview.Binding;
using PluginManager.ViewModel.PluginOverview;
using Whathecode.System.ComponentModel.NotifyPropertyFactory.Attributes;
using Whathecode.System.Windows.Aspects.ViewModel;
using Whathecode.System.Windows.Input.CommandFactory.Attributes;


namespace PluginManager.ViewModel.AppOverview
{
	[ViewModel( typeof( Binding.Properties ), typeof( Commands ) )]
	public class AppOverviewViewModel
	{
		public delegate void PluginOverviewEventHandler( PluginViewModel pluginViewModel, Guid abcPluginGuid );

		//<summary>
		//  Event which is triggered when plug-in being installed.
		//</summary>
		public event PluginOverviewEventHandler InstallingPluginEvent;

		[NotifyProperty( Binding.Properties.Applications )]
		public ObservableCollection<Application> Applications { get; set; }

		[NotifyProperty( Binding.Properties.SelectedApplication )]
		public Application SelectedApplication { get; set; }

		[NotifyProperty( Binding.Properties.Filters )]
		public List<string> Filters { get; private set; }

		[NotifyProperty( Binding.Properties.SelectedFilter )]
		public string SelectedFilter { get; private set; }

		[NotifyProperty( Binding.Properties.CurrentPlugins )]
		public PluginOverviewViewModel CurrentPlugins { get; private set; }

		[NotifyPropertyChanged( Binding.Properties.SelectedFilter )]
		public void OnSelectedFilterChanged( string oldFilter, string newFilter )
		{
			PluginState state;
			if ( Enum.TryParse( newFilter, out state ) )
			{
				FilterApps( state );
			}
			else
			{
				FilterApps();
			}
		}

		readonly PluginManifest _pluginManifest;
		readonly Dictionary<Application, List<PluginManifestPlugin>> _applicationPlugins;

		public AppOverviewViewModel( PluginManifest pluginManifest )
		{
			Applications = new ObservableCollection<Application>();
			_applicationPlugins = new Dictionary<Application, List<PluginManifestPlugin>>();
			_pluginManifest = pluginManifest;

			Filters = Enum.GetNames( typeof( PluginState ) ).ToList();
			Filters.Insert( 0, "All" );

			Populate( pluginManifest );
		}

		public void Populate( PluginManifest pluginManifest )
		{
			Mouse.OverrideCursor = Cursors.Wait;

			// Set the filter and show applications + plug-ins.
			SelectedFilter = Filters.First();

			CurrentPlugins.InstallingPluginEvent += ( model, guid ) => InstallingPluginEvent( model, guid );

			Mouse.OverrideCursor = Cursors.Arrow;
		}

		// If state is not provided no filter is used.
		void FilterApps( PluginState state = 0 )
		{
			var availableApps = _pluginManifest.Application.Where( application => _pluginManifest.HasAnyPluginByState( new Guid( application.Guid ), state ) ).ToList();

			Applications.Clear();
			_applicationPlugins.Clear();
			availableApps.ForEach( manifestApplication =>
			{
				var appPlugins = _pluginManifest.GivePlugins( new Guid( manifestApplication.Guid ), state ).ToList();
				var application = new Application( manifestApplication, appPlugins );
				Applications.Add( application );
				_applicationPlugins.Add( application, appPlugins );
			} );
			
			SelectedApplication = Applications.First();
			CurrentPlugins = new PluginOverviewViewModel( _applicationPlugins[ SelectedApplication ] );
		}

		[CommandExecute( Commands.SelectApplication )]
		public void SelectApplication()
		{
			CurrentPlugins = new PluginOverviewViewModel( _applicationPlugins[ SelectedApplication ].ToList() );
		}
	}
}