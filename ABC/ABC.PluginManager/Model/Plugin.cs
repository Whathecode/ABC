
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace PluginManager.Model
{
	[XmlTypeAttribute(AnonymousType = true)]
	public class Plugin : PluginElement
	{
		public Plugin()
		{
			Interruptions = new List<Configuration>();
			Vdm = new List<Configuration>();
			Persistence = new List<Configuration>();
			Icon = new Uri("pack://application:,,,/View/icons/defaultApp.png").AbsolutePath;
			TimeStamp = DateTime.Now.ToShortDateString();
		}

		[XmlArrayItemAttribute("Configuration", IsNullable = false)]
		public List<Configuration> Interruptions { get; set; }


		[XmlArrayItemAttribute("Configuration", IsNullable = false)]
		public List<Configuration> Vdm { get; set; }


		[XmlArrayItemAttribute("Configuration", IsNullable = false)]
		public List<Configuration> Persistence { get; set; }

		public string Icon { get; set; }

		public string CompanyName { get; set; }
	}
}
