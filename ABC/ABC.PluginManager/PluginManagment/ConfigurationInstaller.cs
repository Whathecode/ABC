using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using PluginManager.Common;
using PluginManager.Model;


namespace PluginManager.PluginManagment
{
	class PluginInstaller
	{
		public delegate void PluginInstallerEventHandler( Configuration configuration, string message );

		/// <summary>
		///   Event which is triggered at the start when plug-in installation process has ended.
		/// </summary>
		public event PluginInstallerEventHandler PluginInstalledEvent;

		readonly Configuration _configuration;
		readonly string _configurationFilePath;
		readonly string _configName;

		public PluginInstaller( Configuration configuration, PluginType configurationType )
		{
			_configuration = configuration;
			_configName = _configuration.ConfigFile.Substring( _configuration.ConfigFile.LastIndexOf( '/' ) + 1 );
			switch ( configurationType )
			{
				case PluginType.Interruptions:
					_configurationFilePath = Path.Combine( App.InterruptionsPluginLibrary, _configName );
					break;
				case PluginType.Vdm:
					_configurationFilePath = Path.Combine( App.VdmPluginLibrary, _configName );
					break;
				case PluginType.Persistence:
					_configurationFilePath = Path.Combine( App.PersistencePluginLibrary, _configName );
					break;
			}
		}

		public void Install()
		{
			if ( _configuration.Installer != null )
			{
				var webClient = new WebClient();
				// TODO: Test installation process with a plug-in which needs an installer.
				// TODO: When plug-in has a separate installer another event should be triggered? 
				//webClient.DownloadFileCompleted += PluginDownloadHandler;
				webClient.DownloadFileAsync( new Uri( _configuration.Installer ), _configurationFilePath );
			}
			else if ( _configuration.ConfigFile != null )
			{
				var webClient = new WebClient();
				webClient.DownloadFileCompleted += PluginDownloadHandler; 
				webClient.DownloadFileAsync( new Uri( _configuration.ConfigFile ), _configurationFilePath );
			}
		}

		void PluginDownloadHandler( object sender, AsyncCompletedEventArgs asyncCompletedEventArgs )
		{
			if ( !asyncCompletedEventArgs.Cancelled )
			{
				PluginInstalledEvent( _configuration, _configName + " was downloaded properly." );
			}
			else
			{
				PluginInstalledEvent( _configuration, "During download of " + _configName + " error occurred: " +  asyncCompletedEventArgs.Error.Message );
			}
		}
	}
}