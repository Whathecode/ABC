using System;


namespace ABC.Common
{
	/// <summary>
	///   Exception which is thrown when one of the plugins throws an exception.
	/// </summary>
	public class PluginException<TPlugin> : Exception
	{
		readonly TPlugin _plugin;


		/// <summary>
		///   Create a new exception indicating a plugin threw an exception.
		/// </summary>
		/// <param name = "message">The error message that explains the reason for the exception.</param>
		/// <param name = "plugin">The plugin that threw the exception.</param>
		/// <param name = "exception">The exception thrown by the plugin.</param>
		public PluginException( string message, TPlugin plugin, Exception exception )
			: base( message, exception )
		{
			_plugin = plugin;
		}
	}
}
