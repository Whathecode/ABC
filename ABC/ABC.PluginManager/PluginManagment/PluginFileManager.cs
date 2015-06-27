using System;
using System.IO;
using System.Net;
using PluginManager.Model;


namespace PluginManager.PluginManagment
{
	class PluginFileManager
	{
		const string PluginExtention = ".plugin";
		const string DllExtention = ".dll";
		const string XmlExtention = ".xml";
		const string TempDirectory = "temp";

		const string CannotFindAPlugInToDelete = "Cannot find a plug-in to delete. ";

		const string RemotePluginPath = @"http://members.upcpoczta.pl/z.grondziowska/plugins/";
		readonly string _pluginDownloadPath;
		readonly string _pluginTempPath;
		readonly string _pluginPath;

		readonly FileInfo _pluginfileInfo;

		readonly WebClient _webClient;


		public PluginFileManager( Guid abcPluginGuid, PluginType abcPluginType )
		{
			// Plug-in paths setup.
			_pluginDownloadPath = RemotePluginPath + abcPluginGuid + PluginExtention;
			_pluginTempPath = Path.Combine( GetPluginDirectory( abcPluginType ), TempDirectory, abcPluginGuid.ToString() ) + GetPluginExtention( abcPluginType );
			_pluginPath = Path.Combine( GetPluginDirectory( abcPluginType ), abcPluginGuid.ToString() ) + GetPluginExtention( abcPluginType );

			_pluginfileInfo = new FileInfo( _pluginTempPath );

			if ( !Directory.Exists( _pluginTempPath ) )
			{
				Directory.CreateDirectory( _pluginfileInfo.DirectoryName );
			}

			_webClient = new WebClient();
			_webClient.DownloadFileCompleted += ( sender, args ) =>
			{
				// When download is completed and performed properly move to final destination, otherwise delete. 
				if ( args.Error == null && !args.Cancelled )
				{
					_pluginfileInfo.MoveTo( Path.Combine( _pluginfileInfo.Directory.Parent.FullName, _pluginfileInfo.Name ) );
				}
				else
				{
					File.Delete( _pluginTempPath );
				}
			};
		}

		static string GetPluginDirectory( PluginType type )
		{
			return type == PluginType.Persistence
				? App.PersistencePluginLibrary
				: type == PluginType.Interruption
					? App.InterruptionsPluginLibrary
					: App.VdmPluginLibrary;
		}

		static string GetPluginExtention( PluginType type )
		{
			return type == PluginType.Persistence || type == PluginType.Interruption ? DllExtention : XmlExtention;
		}

		public void Delete()
		{
			try
			{
				File.Delete( _pluginPath );
			}
			catch ( Exception exception )
			{

				Console.WriteLine( CannotFindAPlugInToDelete + exception.Message );
				throw;
			}
		}

		public void DownloadAsync()
		{
			_webClient.DownloadFileAsync( new Uri( _pluginDownloadPath ), _pluginTempPath );
		}

		public void Download()
		{
			_webClient.DownloadFile( new Uri( _pluginDownloadPath ), _pluginTempPath );
		}
	}
}