using System;
using System.Collections.Generic;
using System.Xml.Serialization;


namespace PluginManager.Model
{
	[XmlTypeAttribute( AnonymousType = true )]
	public class Plugin : PluginElement
	{
		public Plugin()
		{
			Interruptions = new List<Configuration>();
			Vdm = new List<Configuration>();
			Persistence = new List<Configuration>();
			TimeStamp = DateTime.Now.ToShortDateString();
		}

		public Plugin( Plugin plugin ) : this()
		{
			Icon = plugin.Icon;
			TimeStamp = plugin.TimeStamp;
			CompanyName = plugin.CompanyName;
			Id = plugin.Id;
			Name = plugin.Name;
			Version2 = plugin.Version2;
			Author = plugin.Author;
		}

		[XmlArrayItemAttribute( "Configuration", IsNullable = false )]
		public List<Configuration> Interruptions { get; set; }


		[XmlArrayItemAttribute( "Configuration", IsNullable = false )]
		public List<Configuration> Vdm { get; set; }


		[XmlArrayItemAttribute( "Configuration", IsNullable = false )]
		public List<Configuration> Persistence { get; set; }

		public string Icon { get; set; }

		public string CompanyName { get; set; }
	}
}