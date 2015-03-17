using System;
using PluginManager.Common;
using PluginManager.Model;
using PluginManager.PluginManagment;
using PluginManager.ViewModel.PluginOverview.Binding;
using Whathecode.System.ComponentModel.NotifyPropertyFactory.Attributes;
using Whathecode.System.Extensions;
using Whathecode.System.Windows.Aspects.ViewModel;


namespace PluginManager.ViewModel.PluginOverview
{
	[ViewModel( typeof( PluginProperties ), typeof( PluginCommands ) )]
	public class PluginViewModel
	{
		public delegate void PluginEventHandler( PluginViewModel pluginViewModel, Guid abcPlugin );

		/// <summary>
		///   Event which is triggered when plug-in being installed.
		/// </summary>
		public event PluginEventHandler IntallingPluginEvent;

		public Guid Guid { get; private set; }

		[NotifyProperty( PluginProperties.Author )]
		public string Author { get; private set; }

		[NotifyProperty( PluginProperties.Description )]
		public string Description { get; private set; }

		[NotifyProperty( PluginProperties.Version )]
		public Version Version { get; private set; }

		public PluginType PluginType { get; private set; }

		[NotifyProperty( PluginProperties.Icon )]
		public string Icon { get; private set; }

		[NotifyProperty( PluginProperties.State )]
		public PluginState State { get; set; }

		readonly PluginManifestPlugin _plugin;

		public PluginViewModel( PluginManifestPlugin plugin )
		{
			Guid = plugin.Guid;
			Author = plugin.Author;
			Description = plugin.Description;
			Version = new Version( plugin.Version );
			Description = plugin.Description;
			PluginType = plugin.Type;
			Icon = plugin.Icon;
			State = plugin.PluginState;

			_plugin = plugin;
		}

		public void DownloadAbcPlugins()
		{
			_plugin.AbcPlugins.ForEach( abcplugin =>
			{
				var downloader = new PluginDownloader( Guid, PluginType );

				// TODO: Send event to install plug-in. 
				downloader.PluginDownloadedEvent += ( plugin, args ) => IntallingPluginEvent( this, new Guid( abcplugin.Guid ) );
				downloader.DownloadAsync();
			} );
		}
	}
}