using System;
using System.Linq;
using PluginManager.Common;
using PluginManager.Model;
using PluginManager.PluginManagement;
using PluginManager.ViewModel.Plugin.Binding;
using Whathecode.System.ComponentModel.NotifyPropertyFactory.Attributes;
using Whathecode.System.Extensions;
using Whathecode.System.Windows.Aspects.ViewModel;
using Whathecode.System.Windows.Input.CommandFactory.Attributes;


namespace PluginManager.ViewModel.Plugin
{
	[ViewModel( typeof( Binding.Properties ), typeof( Commands ) )]
	public class PluginViewModel
	{
		public delegate void PluginEventHandler( PluginViewModel pluginViewModel, Guid abcPlugin );

		/// <summary>
		///   Event which is triggered when plug-in being configured.
		/// </summary>
		public event EventHandler ConfiguringPluginEvent;

		[NotifyProperty( Binding.Properties.Author )]
		public string Author { get; private set; }

		[NotifyProperty( Binding.Properties.Description )]
		public string Description { get; private set; }

		[NotifyProperty( Binding.Properties.Version )]
		public Version Version { get; private set; }

		public PluginType PluginType { get; private set; }

		[NotifyProperty( Binding.Properties.Icon )]
		public string Icon { get; private set; }

		[NotifyProperty( Binding.Properties.State )]
		public PluginState State { get; set; }

		[NotifyProperty( Binding.Properties.IsConfigurable )]
		public bool IsConfigurable { get; set; }

		readonly PluginManifestPlugin _plugin;

		public Guid Guid
		{
			get { return new Guid( _plugin.AbcPlugins.First().Guid ); }
		}

		// If one plug-in (plug-in manager) is consisted from many ABC sub plug-ins the first one should have a configuration logic.
		public string PluginPath
		{
			get { return _plugin.AbcPlugins.First().PluginPath; }
		}

		public PluginViewModel( PluginManifestPlugin plugin )
		{
			Author = plugin.Author;
			Description = plugin.Description;
			Version = new Version( plugin.Version );
			Description = plugin.Description;
			PluginType = plugin.Type;
			Icon = plugin.Icon;
			State = plugin.PluginState;
			IsConfigurable = plugin.IsConfigurable;

			_plugin = plugin;
		}

		[CommandExecute( Commands.Download )]
		public void Download()
		{
			_plugin.AbcPlugins.ForEach( abcplugin =>
			{
				var guid = new Guid( abcplugin.Guid );
				var manager = new PluginFileManager( guid, PluginType );

				manager.DownloadAsync();
			} );
		}

		[CommandExecute( Commands.Delete )]
		public void Delete()
		{
			_plugin.AbcPlugins.ForEach( abcplugin =>
			{
				var guid = new Guid( abcplugin.Guid );
				var manager = new PluginFileManager( guid, PluginType, PluginPath );

				manager.Delete();
			} );
		}

		[CommandExecute( Commands.Configure )]
		public void Configure()
		{
			ConfiguringPluginEvent( this, new EventArgs() );
		}
	}
}