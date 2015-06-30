using System;
using PluginManager.Common;


namespace PluginManager.Model
{
	public partial class PluginManifestPlugin
	{
		public PluginManifestPlugin()
		{
			Guid = Guid.NewGuid();
		}

		public PluginState PluginState { get; set; }
		
		public Guid Guid { get; set; }

		public bool IsConfigurable { get; set; }
	}

	public partial class PluginManifestPluginAbcPlugin
	{
		public string PluginPath { get; set; }
	}
}