using System;


namespace PluginManager.Common
{
	[Flags]
	public enum PluginState
	{
		/// <summary>
		/// Show available plug-ins that are not installed as applications.
		/// </summary>
		Availible = 1,

		/// <summary>
		/// Show only installed plug-ins grouped as applications.
		/// </summary>
		Installed = 2,

		/// <summary>
		/// Plug-ins that are installed but out of date.
		/// </summary>
		Updates = 4
	}
}