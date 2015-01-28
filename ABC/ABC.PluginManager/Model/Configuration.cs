using System.Collections.Generic;
using PluginManager.Common;


namespace PluginManager.Model
{
	[System.Xml.Serialization.XmlTypeAttribute( AnonymousType = true )]
	public class Configuration : PluginElement
	{
		public Configuration(){}
		public Configuration( List<string> supportedVersions, string author, string version, string timeStamp, PluginState state )
		{
			SupportedVersions = supportedVersions;
			Author = author;
			Version = version;
			TimeStamp = timeStamp;
			State = state;
		}

		public string ConfigFile { get; set; }

		[System.Xml.Serialization.XmlArrayItemAttribute( "Version", IsNullable = false )]
		public List<string> SupportedVersions { get; set; }

		public string Icon { get; set; }

		public PluginState State { get; set; }
	}
}