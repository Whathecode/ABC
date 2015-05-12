using System;
using ABC.Plugins;


namespace PluginManager.PluginManagment
{
	public class PluginEventArgs : EventArgs
	{
		public AssemblyInfo PluginAssemblyInfo { get; private set; }

		public PluginEventArgs( AssemblyInfo pluginAssemblyInfo )
		{
			PluginAssemblyInfo = pluginAssemblyInfo;
		}
	}
}