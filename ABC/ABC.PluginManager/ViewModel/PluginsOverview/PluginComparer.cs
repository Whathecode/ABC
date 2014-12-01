using System;
using System.Collections.Generic;
using PluginManager.ViewModel.PluginDetails;


namespace PluginManager.ViewModel.PluginsOverview
{
	class PluginComparer : IEqualityComparer<PluginDetailsViewModel>
	{
		public bool Equals( PluginDetailsViewModel a, PluginDetailsViewModel b )
		{
			return a.Plugin.Name.Equals( b.Plugin.Name, StringComparison.CurrentCultureIgnoreCase )
			       && a.Plugin.CompanyName.Equals( b.Plugin.CompanyName, StringComparison.CurrentCultureIgnoreCase );
		}

		public int GetHashCode( PluginDetailsViewModel plugin )
		{
			return plugin.Plugin.Name.GetHashCode();
		}
	}
}
