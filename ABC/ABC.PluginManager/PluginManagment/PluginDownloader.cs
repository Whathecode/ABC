using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using PluginManager.Model;


namespace PluginManager.PluginManagment
{
	class PluginDownloader
	{
		public delegate void PluginDownloadedEventHandler( Guid plugin, AsyncCompletedEventArgs eventArgs );

		/// <summary>
		///   Event which is triggered at the start when plug-in installation process has ended.
		/// </summary>
		public event PluginDownloadedEventHandler PluginDownloadedEvent;

		const string DownloadPath = @"http:/members.upcpoczta.pl/z.grondziowska/Plugins/";
		readonly string _pluginDownloadPath;
		readonly string _pluginSavePath;

		readonly WebClient _webClient;

		public PluginDownloader( Guid abcPluginGuid, PluginType abcPluginType )
		{
			_webClient = new WebClient();
			_webClient.DownloadFileCompleted += ( sender, args ) => PluginDownloadedEvent( abcPluginGuid, args );

			_pluginDownloadPath = DownloadPath + abcPluginGuid;

			_pluginSavePath = Path.Combine( abcPluginType == PluginType.Interruption
				? App.InterruptionsPluginLibrary
				: abcPluginType == PluginType.Persistence
					? App.PersistencePluginLibrary
					: App.VdmPluginLibrary, abcPluginGuid.ToString() );
		}

		public void DownloadAsync()
		{
			_webClient.DownloadFileAsync( new Uri( _pluginDownloadPath ), _pluginSavePath );
		}

		public void Download()
		{
			_webClient.DownloadFile( new Uri( _pluginDownloadPath ), _pluginSavePath );
		}
	}
}