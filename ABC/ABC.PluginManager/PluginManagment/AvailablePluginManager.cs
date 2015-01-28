using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Xml;
using System.Xml.Serialization;
using PluginManager.Common;
using PluginManager.Model;


namespace PluginManager.PluginManagment
{
	public class AvailablePluginManager
	{
		private const string AppsFeed = "http://members.upcpoczta.pl/z.grondziowska/AvailableAppsSmall.xml";

		public List<Plugin> AvailablePlugins { get; private set; }

		public AvailablePluginManager()
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

		public List<Plugin> GiveAvalibleApps()
		{
			if (!HasInternetConnection())
			{
				return null;
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

			var availablePlugins = new List<Plugin>(availibleApps.AvailablePlugins);
			availablePlugins = PluginManagmentHelper.MergePluginsByName( availablePlugins );

			availablePlugins.ForEach( plugin =>
			{
				plugin.Vdm.ForEach( vdm => vdm.State = PluginState.Availible );
				plugin.Persistence.ForEach( persistence => persistence.State = PluginState.Availible );
				plugin.Interruptions.ForEach( interruption => interruption.State = PluginState.Availible );
			} );

			PluginManagmentHelper.SortByName( ref availablePlugins );
			
			AvailablePlugins = availablePlugins;
			return availablePlugins;
		}
	}
}