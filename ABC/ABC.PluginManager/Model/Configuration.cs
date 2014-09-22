using System;
using System.Collections.Generic;

namespace PluginManager.Model
{
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
	public class Configuration : PluginElement
	{
		public Configuration()
		{
			Icon = new Uri("pack://application:,,,/View/icons/conf.png").AbsolutePath;
			SupportedVersions = new List<string> { "-" };
		}

		public string ConfigFile { get; set; }

		public string Installer { get; set; }

		[System.Xml.Serialization.XmlArrayItemAttribute("Version", IsNullable = false)]
		public List<string> SupportedVersions { get; set; }

		public string Icon { get; set; }
	}
}