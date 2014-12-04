using System;

namespace PluginManager.Common
{
	[Flags]
	public enum PluginType
	{
		/// <summary>
		/// Show only installed plug-ins grouped as applications.
		/// </summary>
		Interruptions,

		/// <summary>
		/// Show updates for installed plug-ins.
		/// </summary>
		Vdm,

		/// <summary>
		/// Show available plug-ins that are not installed as applications.
		/// </summary>
		Persistence
	}
}
