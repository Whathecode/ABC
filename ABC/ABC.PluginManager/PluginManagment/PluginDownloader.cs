using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using PluginManager.Common;
using PluginManager.Model;


namespace PluginManager.PluginManagment
{
	class PluginDownloader
	{
		public delegate void PluginDownloadedEventHandler( Configuration configuration, AsyncCompletedEventArgs eventArgs );

		/// <summary>
		///   Event which is triggered at the start when plug-in installation process has ended.
		/// </summary>
		public event PluginDownloadedEventHandler PluginDownloadedEvent;

		readonly Configuration _configuration;
		readonly string _configurationFilePath;
		readonly WebClient _webClient;

		public PluginDownloader( Configuration configuration, PluginType configurationType )
		{
			_webClient = new WebClient();
			_webClient.DownloadFileCompleted += ( sender, args ) => PluginDownloadedEvent( _configuration, args );

			_configuration = configuration;
			var configName = _configuration.ConfigFile.Substring( _configuration.ConfigFile.LastIndexOf( '/' ) + 1 );
			
			_configurationFilePath = Path.Combine( configurationType == PluginType.Interruptions ? 
				App.InterruptionsPluginLibrary : configurationType == PluginType.Persistence ? 
				App.PersistencePluginLibrary : App.VdmPluginLibrary, configName );

			
		}

		public void DownloadAsync()
		{
			_webClient.DownloadFileAsync( new Uri( _configuration.ConfigFile ), _configurationFilePath );
		}

		public void Download()
		{
			_webClient.DownloadFile( new Uri( _configuration.ConfigFile ), _configurationFilePath );
		}
	}
}