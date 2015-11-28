using System.IO;
using System.Net;
using System.Xml;
using System.Xml.Serialization;
using PluginManager.Model;


namespace PluginManager.PluginManagment
{
	public class PluginManifestManager
	{
		const string AppsFeed = "http://domi.frwaw.itu.dk/pluginManager/PluginManifest.xml";

		public PluginManifest PluginManifest { get; private set; }

		public PluginManifestManager()
		{
			RefreshManifest();
		}

		static bool HasInternetConnection()
		{
			try
			{
				using ( var client = new WebClient() )
				using ( client.OpenRead( "http://www.google.com" ) )
				{
					return true;
				}
			}
			catch
			{
				return false;
			}
		}

		public void RefreshManifest()
		{
			if ( !HasInternetConnection() )
			{
				return;
			}

			// Retrieving the available applications stream.
			Stream appsStream = null;
			while ( appsStream == null )
			{
				var client = new WebClient();
				appsStream = client.OpenRead( AppsFeed );
			}

			var serializer = new XmlSerializer( typeof( PluginManifest ) );
			var reader = XmlReader.Create( appsStream );

			// Use Deserialize method to restore the object's state.
			PluginManifest = (PluginManifest)serializer.Deserialize( reader );
		}
	}
}