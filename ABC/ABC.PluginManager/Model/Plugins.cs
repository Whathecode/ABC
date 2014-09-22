
using System.Collections.Generic;

namespace PluginManager.Model
{
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
	[System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
	public class Plugins
	{
		[System.Xml.Serialization.XmlElementAttribute("Plugin")]
		public List<Plugin> AvailablePlugins { get; set; }
	}
}
