using System;


namespace PluginManager.common
{
	[Flags]
	public enum PluginState
	{
		/// <summary>
		/// Show only installed plug-ins grouped as applications.
		/// </summary>
		Installed = 1,

		/// <summary>
		/// Show updates for installed plug-ins.
		/// </summary>
		Updates = 3,

		/// <summary>
		/// Show available plug-ins that are not installed as applications.
		/// </summary>
		Availible = 4
	}
}