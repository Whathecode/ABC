using System;


namespace PluginManager.common
{
	[Flags]
	public enum PluginFilter
	{
		/// <summary>
		/// Show only installed plug-ins grouped as applications.
		/// </summary>
		Installed = 1,

		/// <summary>
		/// Show available plug-ins that are not installed as applications.
		/// </summary>
		Availible = 2,

		/// <summary>
		/// Show updates for installed plug-ins.
		/// </summary>
		Updates = 4,
	}
}