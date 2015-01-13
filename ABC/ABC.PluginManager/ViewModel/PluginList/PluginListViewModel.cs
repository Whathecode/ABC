using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using PluginManager.Common;
using PluginManager.Model;
using PluginManager.ViewModel.PluginDetails;
using PluginManager.ViewModel.PluginList.Binding;
using Whathecode.System.ComponentModel.NotifyPropertyFactory.Attributes;
using Whathecode.System.Windows.Aspects.ViewModel;
using Whathecode.System.Windows.Input.CommandFactory.Attributes;


namespace PluginManager.ViewModel.PluginList
{
	[ViewModel( typeof( Binding.Properties ), typeof( Commands ) )]
	public class PluginListViewModel
	{
		[NotifyProperty( Binding.Properties.ApplicationDatails )]
		public PluginOverviewViewModel PluginOverviewViewModel { get; set; }

		[NotifyProperty( Binding.Properties.SelectedConfigurationItem )]
		public Configuration SelectedConfigurationItem { get; set; }

		[NotifyPropertyChanged( Binding.Properties.SelectedConfigurationItem )]
		public void OnSelectedConfigurationItemChanged( Configuration oldConfiguration, Configuration newConfiguration )
		{
			if ( newConfiguration != null )
			{
				PluginOverviewViewModel.SelectedConfigurationItem = newConfiguration;
			}
		}

		public string PluginListName { get; private set; }

		[NotifyProperty( Binding.Properties.PluginList )]
		public ObservableCollection<Configuration> PluginList { get; private set; }

		public PluginType PluginType { get; private set; }

		public PluginListViewModel( PluginType pluginType, List<Configuration> configurations, PluginOverviewViewModel pluginOverviewViewModel )
		{
			PluginType = pluginType;
			PluginListName = PluginType.ToString();

			// Check if configurations data contains all values.
			configurations.ForEach( configuration =>
			{
				configuration.Icon = configuration.Icon ?? new Uri( "pack://application:,,,/View/icons/conf.png" ).AbsolutePath;
				configuration.Version = configuration.Version ?? "1.0";
				configuration.Author = configuration.Author ?? "Unknown author";
				configuration.SupportedVersions = configuration.SupportedVersions != null && configuration.SupportedVersions.Any() 
					? configuration.SupportedVersions : new List<string> { "Any" };
			} );

			PluginList = new ObservableCollection<Configuration>( configurations );
			PluginOverviewViewModel = pluginOverviewViewModel;
		}

		[CommandExecute( Commands.InstallPlugin )]
		public void InstallPlugin()
		{
			PluginOverviewViewModel.InstallPlugin( this );
		}
	}
}