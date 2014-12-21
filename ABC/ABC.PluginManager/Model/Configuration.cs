using System.Collections.Generic;
using PluginManager.common;


namespace PluginManager.Model
{
	[System.Xml.Serialization.XmlTypeAttribute( AnonymousType = true )]
	public class Configuration : PluginElement
	{
		public string ConfigFile { get; set; }

		public string Installer { get; set; }

		[System.Xml.Serialization.XmlArrayItemAttribute( "Version", IsNullable = false )]
		public List<string> SupportedVersions { get; set; }

		public string Icon { get; set; }

		public PluginState State { get; set; }
	}
}