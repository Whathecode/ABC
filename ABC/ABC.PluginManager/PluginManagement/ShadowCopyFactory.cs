using System;
using System.IO;
using System.Reflection;


namespace PluginManager.PluginManagement
{
	public class ShadowCopyFactory<T>
	{
		public static T Create( string pluginFolderPath, object[] arguments = null, string shadowCopyName = "ShadowCopyCache", string domainName = "Host_AppDomain" )
		{
			var pluginCacheFolderPath = Path.Combine( pluginFolderPath, shadowCopyName );

			// Check if plug-in and shadow copy folders already exist, create otherwise.
			Directory.CreateDirectory( pluginFolderPath );
			Directory.CreateDirectory( pluginCacheFolderPath );

			// This creates a ShadowCopy of the MEF DLL's.
			var setup = new AppDomainSetup
			{
				CachePath = pluginCacheFolderPath,
				ShadowCopyFiles = "true",
				ShadowCopyDirectories = pluginFolderPath
			};
			// Create separate domain.
			var domain = AppDomain.CreateDomain( domainName, AppDomain.CurrentDomain.Evidence, setup );

			// Create new PluginContainer using new separate domain.
			var pluginProvider = (T)domain.CreateInstanceAndUnwrap( typeof( T ).Assembly.FullName,
				typeof( T ).FullName, false, BindingFlags.CreateInstance, null, arguments, null, null );

			return pluginProvider;
		}
	}
}