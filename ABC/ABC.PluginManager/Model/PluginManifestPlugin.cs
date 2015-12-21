using System;
using System.Configuration;
using System.Reflection;
using PluginManager.Common;


namespace PluginManager.Model
{
	public partial class PluginManifestPlugin
	{
		public PluginManifestPlugin()
		{
			Guid = Guid.NewGuid();

			var _config = ConfigurationManager.OpenExeConfiguration( Assembly.GetExecutingAssembly().Location );
			if ( _config == null )
			{
				return;
			}

			var _settings = _config.AppSettings;
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