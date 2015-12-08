using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using PluginManager.Common;
using PluginManager.PluginManagment;
using Whathecode.System.Extensions;


namespace PluginManager.Model
{
	public partial class PluginManifest : MarshalByRefObject
	{
		public IEnumerable<PluginManifestPlugin> GivePlugins( Guid applicationGuid, PluginState state = 0 )
		{
			return state == 0
				? Plugin.Where( plugin => new Guid( plugin.ApplicationGuid ) == applicationGuid )
				: Plugin.Where( plugin => new Guid( plugin.ApplicationGuid ) == applicationGuid && plugin.PluginState == state ).ToList();
		}

		public bool HasAnyPluginByState( Guid applicationGuid, PluginState state )
		{
			return Plugin.Any( plugin => new Guid( plugin.ApplicationGuid ) == applicationGuid && plugin.PluginState == PluginState.Availible );
		}

		public void ChceckPluginState( Dictionary<PluginType, PluginProvider> providers )
		{
			// Check which plug-ins, applications are installed.
			Plugin.ForEach( plugin =>
			{
				var provider = providers[ plugin.Type ];
				if ( provider == null )
					return;

				// Check if all ABC plug-ins are installed to indicate manager plug-in state.
				var installed = plugin.AbcPlugins.Where( abcPlugin => provider.IsInstalled( new Guid( abcPlugin.Guid ) ) ).ToList();
				plugin.AbcPlugins.ForEach( abcplugin => abcplugin.PluginPath = provider.GetPluginPath( new Guid( abcplugin.Guid ) ) );
				plugin.IsConfigurable = provider.IsPluginConfigurable( new Guid( plugin.AbcPlugins.First().Guid ) );
				// All ABC plug-ins are installed.
				if ( installed.Count() == plugin.AbcPlugins.Count() )
				{
					plugin.PluginState = PluginState.Installed;

					//Check if any of ABC plug-ins is out of date.
					if ( plugin.AbcPlugins.Any( abcplugin => provider.CompareVersion( new Guid( abcplugin.Guid ), new Version( abcplugin.Version ) ) != 0 ) )
					{
						plugin.PluginState = PluginState.Updates;
					}
				}
				// Not all ABC plug-ins are installed.
				else if ( installed.Any() )
				{
					plugin.PluginState = PluginState.Updates;
				}
				// None of ABC plug-ins are installed.
				else
				{
					plugin.PluginState = PluginState.Availible;
				}
			} );
		}

		[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.Infrastructure)]
		public override object InitializeLifetimeService()
		{
			return null;
		}
	}
}