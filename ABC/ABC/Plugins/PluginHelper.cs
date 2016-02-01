using System;


namespace ABC.Plugins
{
	public static class PluginHelper<TPlugin>
	{
		/// <summary>
		///   Safely invoke an action on a plugin, catching any exceptions which occur and redirecting it to <see cref="PluginException{T}" />.
		/// </summary>
		/// <param name = "plugin">The trigger to invoke the action on.</param>
		/// <param name = "invocation">The action which needs to be invoked.</param>
		public static void SafePluginInvoke( TPlugin plugin, Action<TPlugin> invocation )
		{
			try
			{
				invocation( plugin );
			}
			catch ( Exception ex )
			{
				string message = $"The plugin \"{plugin.GetType()}\" threw an exception.";

				throw new PluginException<TPlugin>( message, plugin, ex );
			}
		}

		/// <summary>
		///   Safely invoke a function on a plugin, catching any exceptions which occur and redirecting it to <see cref="PluginException{T}" />.
		/// </summary>
		/// <param name = "plugin">The trigger to invoke the function on.</param>
		/// <param name = "invocation">The function which needs to be invoked.</param>
		public static TReturn SafePluginInvoke<TReturn>( TPlugin plugin, Func<TPlugin, TReturn> invocation )
		{
			try
			{
				return invocation( plugin );
			}
			catch ( Exception ex )
			{
				string message = $"The plugin \"{plugin.GetType()}\" threw an exception.";

				throw new PluginException<TPlugin>( message, plugin, ex );
			}
		}
	}
}
