using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using PluginManager.Model;
using PluginManager.ViewModel.Plugin;
using PluginManager.ViewModel.PluginOverview.Binding;
using Whathecode.System.ComponentModel.NotifyPropertyFactory.Attributes;
using Whathecode.System.Extensions;
using Whathecode.System.Windows.Aspects.ViewModel;
using Whathecode.System.Windows.Input.CommandFactory.Attributes;


namespace PluginManager.ViewModel.PluginOverview
{
	[ViewModel( typeof( Binding.Properties ), typeof( Commands ) )]
	public class PluginOverviewViewModel
	{
		public delegate void PluginEventHandler( PluginViewModel pluginViewModel, EventArgs args );

		/// <summary>
		///   Event which is triggered when plug-in being configured.
		/// </summary>
		public event PluginEventHandler ConfiguringPluginEvent;

		[NotifyProperty( Binding.Properties.SelectedPlugins )]
		public ObservableCollection<PluginViewModel> SelectedPlugins { get; private set; }

		[NotifyProperty( Binding.Properties.SelectedPluginIndex )]
		public PluginViewModel SelectedPluginIndex { get; set; }

		[NotifyProperty( Binding.Properties.Plugins )]
		public ObservableCollection<PluginViewModel> Plugins { get; private set; }

		public PluginOverviewViewModel( List<PluginManifestPlugin> plugins )
		{
			Plugins = new ObservableCollection<PluginViewModel>();
			SelectedPlugins = new ObservableCollection<PluginViewModel>();

			plugins.ForEach( plugin =>
			{
				var pluginViewModel = new PluginViewModel( plugin );
				pluginViewModel.ConfiguringPluginEvent += ( sender, args ) => ConfiguringPluginEvent( (PluginViewModel)sender, args );
				Plugins.Add( pluginViewModel );
			} );
		}

		[CommandExecute( Commands.DownloadPlugins )]
		public void DownloadPlugins()
		{
			SelectedPlugins.ForEach( selectedPlugin => selectedPlugin.Download() );
		}
	}
}