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

		const string DownloadPath = @"http://members.upcpoczta.pl/z.grondziowska/plugins/";
		readonly string _pluginDownloadPath;
		readonly string _pluginSavePath;
		const string PluginExtention = ".plugin";
		const string PluginDllExtention = ".dll";
		const string PluginXmlExtention = ".xml";

		readonly WebClient _webClient;

		public PluginDownloader( Guid abcPluginGuid, PluginType abcPluginType )
		{
			_webClient = new WebClient();
			_webClient.DownloadFileCompleted += ( sender, args ) => PluginDownloadedEvent( abcPluginGuid, args );

			_pluginDownloadPath = DownloadPath + abcPluginGuid + PluginExtention;

			switch ( abcPluginType )
			{
				case PluginType.Persistence:
					_pluginSavePath = Path.Combine( App.PersistencePluginLibrary, abcPluginGuid.ToString() ) + PluginDllExtention;
					break;
				case PluginType.Interruption:
					_pluginSavePath = Path.Combine( App.InterruptionsPluginLibrary, abcPluginGuid.ToString() ) + PluginDllExtention;
					break;
				case PluginType.Vdm:
					_pluginSavePath = Path.Combine( App.VdmPluginLibrary, abcPluginGuid.ToString() ) + PluginXmlExtention;
					break;
			}
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