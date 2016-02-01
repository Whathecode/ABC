using System;
using ABC.Plugins;
using ABC.Plugins.Manager;


namespace PluginManager.PluginManagement
{
	[Serializable]
	public class PluginEventArgs : EventArgs
	{
		public PluginInfo PluginAssemblyInfo { get; private set; }

		public PluginEventArgs( PluginInfo pluginAssemblyInfo )
		{
			PluginAssemblyInfo = pluginAssemblyInfo;
		}
	}
}