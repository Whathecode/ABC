using System;
using System.IO;
using ABC.Plugins;
using ABC.Plugins.Manager;


namespace PluginManager.PluginManagement
{
	public class PluginErrorEventArgs : ErrorEventArgs
	{
		public PluginInfo PluginAssemblyInfo { get; private set; }

		public static new PluginErrorEventArgs Empty
		{
			get { return new PluginErrorEventArgs( null, null ); } 
		}

		public PluginErrorEventArgs( Exception exception, PluginInfo pluginAssemblyInfo )
			: base( exception )
		{
			PluginAssemblyInfo = pluginAssemblyInfo;
		}
	}
}
