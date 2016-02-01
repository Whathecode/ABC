using System;
using System.Diagnostics;


namespace ABC.Plugins
{
	public static class PluginHelper
	{
		/// <summary>
		///   Safely invoke an action on a plugin, catching any exceptions which occur and redirecting it to <see cref="PluginException{T}" />.
		/// </summary>
		/// <param name = "plugin">The trigger to invoke the action on.</param>
		/// <param name = "invocation">The action which needs to be invoked.</param>
		public static void SafePluginInvoke<TPlugin>( TPlugin plugin, Action<TPlugin> invocation )
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
		public static TReturn SafePluginInvoke<TPlugin, TReturn>( TPlugin plugin, Func<TPlugin, TReturn> invocation )
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

		/// <summary>
		///   Verifies whether a given process satisfies expected process parameters.
		/// </summary>
		/// <param name="processName">The expected name of the process.</param>
		/// <param name="companyName">The expected company name of the process, as specified in <see cref="FileVersionInfo" />.</param>
		/// <param name="version">
		///   The expected version of the process. When null, all versions are targeted by default.
		///   Version numbers do not need to be complete; 'underlying' versions are also targeted.
		/// </param>
		/// <param name="process">The process to check the given parameters against.</param>
		/// <returns>True when the given parameters match the process; false otherwise.</returns>
		public static bool TargetsProcess( string processName, string companyName, string version, Process process )
		{
			FileVersionInfo info = process.MainModule.FileVersionInfo;
			Version fileVersion = new Version( info.FileMajorPart, info.FileMinorPart, info.FileBuildPart, info.FilePrivatePart );
			return
				processName == process.ProcessName &&
				companyName == info.CompanyName &&
				( version == null || fileVersion.Matches( version ) ); // 'null' matches any version.
		}
	}
}
