using System;
using System.IO;
using ABC.Plugins;


namespace PluginManager.PluginManagment
{
	public class PluginErrorEventArgs : ErrorEventArgs
	{
		public AssemblyInfo PluginAssemblyInfo { get; private set; }

		public static new PluginErrorEventArgs Empty
		{
			get { return new PluginErrorEventArgs( null, null ); } 
		}

		public PluginErrorEventArgs( Exception exception, AssemblyInfo pluginAssemblyInfo )
			: base( exception )
		{
			PluginAssemblyInfo = pluginAssemblyInfo;
		}
	}
}
