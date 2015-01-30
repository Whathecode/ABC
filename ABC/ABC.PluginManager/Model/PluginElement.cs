using System;
using System.Xml.Serialization;

namespace PluginManager.Model
{
	public class PluginElement
	{
		public PluginElement()
		{
			TimeStamp = DateTime.Now.ToShortDateString();
			//Version2 = new Version(Version);
		}

		[XmlAttribute]
		public int Id { get; set; }

		public string Name { get; set; }

		public string Version { get; set; }

		public string Author { get; set; }

		public string TimeStamp { get; set; }

		public Version Version2 { get; set; }
	}
}