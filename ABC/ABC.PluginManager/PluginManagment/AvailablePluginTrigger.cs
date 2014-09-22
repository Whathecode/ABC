using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Xml;
using System.Xml.Serialization;
using PluginManager.Model;


namespace PluginManager.PluginManagment
{
	public class AvailablePluginTrigger
	{
		private const string AppsFeed = "http://members.upcpoczta.pl/z.grondziowska/AvailableAppsSmall.xml";

		// TODO : List of already installed applications, they should be excluded from available applications list.

		/// <summary>
		/// Available application plug-ins.
		/// </summary>
		public List<Plugin> AvailablePlugins { get; private set; }

		public AvailablePluginTrigger()
		{
			GiveAvalibleApps();
		}

		private static bool HasInternetConnection()
		{
			try
			{
				using (var client = new WebClient())
				using (client.OpenRead("http://www.google.com"))
				{
					return true;
				}
			}
			catch
			{
				return false;
			}
		}

		void GiveAvalibleApps()
		{
			if (!HasInternetConnection())
			{
				return;
			}

			// Retrieving the available applications stream.
			Stream appsStream = null;
			while (appsStream == null)
			{
				var client = new WebClient();
				appsStream = client.OpenRead(AppsFeed);
			}

			var serializer = new XmlSerializer(typeof (Plugins));
			var reader = XmlReader.Create(appsStream);

			// Use Deserialize method to restore the object's state.
			var availibleApps = (Plugins) serializer.Deserialize(reader);

			AvailablePlugins = new List<Plugin>(availibleApps.AvailablePlugins);
		}
	}
}