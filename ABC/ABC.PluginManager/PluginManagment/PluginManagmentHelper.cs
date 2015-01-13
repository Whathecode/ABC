using System.Collections.Generic;
using System.Linq;
using PluginManager.Model;
using Whathecode.System.Extensions;


namespace PluginManager.PluginManagment
{
	class PluginManagmentHelper
	{
		public static void SortByName( ref List<Plugin> plugins )
		{
			plugins = plugins.OrderBy( element => element.Name ).ToList();
		}

		public static List<Plugin> MergePluginsByName( IEnumerable<Plugin> plugins )
		{
			var grouped = plugins.GroupBy( plugin => plugin.Name.ToLower() ).ToList();

			var merged = new List<Plugin>();
			grouped.ForEach( groupedElement =>
			{
				if ( groupedElement.Count() > 1 )
				{
					var newPlugin = new Plugin( groupedElement.First() );
					foreach ( var plugin in groupedElement )
					{
						newPlugin.Vdm.AddRange( plugin.Vdm );
						newPlugin.Persistence.AddRange( plugin.Persistence );
						newPlugin.Interruptions.AddRange( plugin.Interruptions );
					}
					merged.Add( newPlugin );
				}
				else
				{
					merged.Add( groupedElement.First() );
				}
			} );
			return merged;
		}

		public static void GiveId( IEnumerable<Plugin> plugins )
		{
			var pluginId = 1;
			plugins.ForEach( plugin =>
			{
				var concatList = plugin.Interruptions.Concat( plugin.Persistence ).Concat( plugin.Vdm );
				var configurationId = 1;
				foreach ( var configuration in concatList )
				{
					configuration.Id = configurationId++;
				}
				plugin.Id = pluginId++;
			} );
		}
	}
}
