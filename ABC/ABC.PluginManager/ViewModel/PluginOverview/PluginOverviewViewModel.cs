using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using PluginManager.Model;
using PluginManager.ViewModel.PluginOverview.Binding;
using Whathecode.System.ComponentModel.NotifyPropertyFactory.Attributes;
using Whathecode.System.Windows.Aspects.ViewModel;
using Whathecode.System.Windows.Input.CommandFactory.Attributes;


namespace PluginManager.ViewModel.PluginOverview
{
	[ViewModel( typeof( Binding.Properties ), typeof( Commands ) )]
	public class PluginOverviewViewModel
	{
		public delegate void PluginEventHandler( PluginViewModel pluginViewModel, Guid abcPluginGuid );

		/// <summary>
		///   Event which is triggered when plug-in being installed.
		/// </summary>
		public event PluginEventHandler InstallingPluginEvent;

		/// <summary>
		///   Event which is triggered when plug-in being uninstalled.
		/// </summary>
		public event PluginEventHandler UninstallingPluginEvent;

		/// <summary>
		///   Event which is triggered when plug-in being updated.
		/// </summary>
		public event PluginEventHandler UpdatingPluginEvent;

		[NotifyProperty( Binding.Properties.SelectedPlugin )]
		public PluginViewModel SelectedPlugin { get; set; }

		[NotifyProperty( Binding.Properties.Plugins )]
		public ObservableCollection<PluginViewModel> Plugins { get; private set; } 

		public PluginOverviewViewModel( List<PluginManifestPlugin> plugins )
		{
			Plugins = new ObservableCollection<PluginViewModel>();
			plugins.ForEach( plugin =>
			{
				var pluginViewModel = new PluginViewModel( plugin );
				pluginViewModel.IntallingPluginEvent += ( viewModel, abcPluginGuid ) => InstallingPluginEvent( viewModel, abcPluginGuid );
				Plugins.Add( pluginViewModel );
			} );
		}

		[CommandExecute( Commands.DownloadPlugin )]
		public void DownloadPlugin()
		{
			SelectedPlugin.DownloadAbcPlugins();
		}
	}
}