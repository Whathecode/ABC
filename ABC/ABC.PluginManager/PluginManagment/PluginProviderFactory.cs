using System;
using System.IO;
using System.Windows;
using PluginManager.Model;


namespace PluginManager.PluginManagment
{
	public static class PluginProviderFactory
	{
		public static PluginProvider CreateProvider( string pluginFolderPath, PluginType pluginType, string filter = null )
		{
			var pluginCacheFolderPath = Path.Combine( pluginFolderPath, "ShadowCopyCache" );

			// Check if plug-in and shadow copy folders already exist, create otherwise.
			if ( !Directory.Exists( pluginFolderPath ) )
			{
				Directory.CreateDirectory( pluginFolderPath );
			}
			if ( !Directory.Exists( pluginCacheFolderPath ) )
			{
				Directory.CreateDirectory( pluginCacheFolderPath );
			}

			// This creates a ShadowCopy of the MEF DLL's.
			var setup = new AppDomainSetup
			{
				CachePath = pluginCacheFolderPath,
				ShadowCopyFiles = "true",
				ShadowCopyDirectories = pluginFolderPath
			};
			// Create separate domain.
			var domain = AppDomain.CreateDomain( "Host_AppDomain", AppDomain.CurrentDomain.Evidence, setup );
			// Create new PluginContainer using new separate domain.
			var persistenceContainer = (PluginProvider)domain.CreateInstanceAndUnwrap( typeof( PluginProvider ).Assembly.FullName,
				typeof( PluginProvider ).FullName );
			
			persistenceContainer.Error += ( sender, args ) => MessageBox.Show( args.GetException().Message );
			persistenceContainer.Initialize( pluginFolderPath, pluginType, filter );
			
			return persistenceContainer;
		}
	}
}
