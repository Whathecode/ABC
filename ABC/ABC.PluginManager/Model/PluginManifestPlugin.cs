using System;
using PluginManager.Common;


namespace PluginManager.Model
{
	public partial class PluginManifestPlugin
	{
		public PluginManifestPlugin()
		{
			Guid = new Guid();
		}

		public PluginState PluginState { get; set; }
		public Guid Guid { get; set; }
	}
}