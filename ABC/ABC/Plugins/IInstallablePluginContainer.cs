using System.Collections.Generic;


namespace ABC.Plugins
{
	public interface IInstallablePluginContainer
	{
		/// <summary>
		/// Reloads container, discovers all newly added plug-ins.
		/// </summary>
		void Reload();

		/// <summary>
		/// Searches plug-ins by name and company name (for now this two parameters are plug-ins identifiers).  
		/// </summary>
		/// <param name="processName">Process name connected to plug-in.</param>
		/// <param name="companyName">Company name connected to application, process.</param>
		/// <returns>Plug-in which implements Installable interface.</returns>
		IInstallable GetInstallablePlugin( string processName, string companyName );

		/// <summary>
		/// Gets all information about plug-ins which are installed.
		/// </summary>
		/// <returns>List of plug-ins information.</returns>
		List<PluginInformation> GetPluginInformation();
	}
}