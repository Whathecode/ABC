﻿using System;
using System.Xml.Serialization;

namespace PluginManager.Model
{
	public class PluginElement
	{
		public PluginElement()
		{
			Name = "Unknown ProcessName";
			Version = "1.0";
			Author = "Unknown Author";
			TimeStamp = DateTime.Now.ToShortDateString();
		}

		[XmlAttribute]
		public int Id { get; set; }

		public string Name { get; set; }

		public string Version { get; set; }

		public string Author { get; set; }

		public string TimeStamp { get; set; }
	}
}