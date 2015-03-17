using System;


namespace ABC.Plugins
{
	public interface IInstallablePluginContainer
	{
		/// <summary>
		/// Reloads container, discovers all newly added plug-ins.
		/// </summary>
		void Reload();

		/// <summary>
		/// Run the install procedure on given plug-in.  
		/// </summary>
		/// <param name="guid">Plug-in identifier.</param>
		/// <returns>True if installing went successfully or is not needed, otherwise false.</returns>
		bool InstallPugin( Guid guid );

		/// <summary>
		/// Run the uninstall procedure on given plug-in.  
		/// </summary>
		/// <param name="guid">Plug-in identifier.</param>
		/// <returns>True if uninstalling went successfully or is not needed, otherwise false.</returns>
		bool UninstallPugin( Guid guid );

		/// <summary>
		/// Gives a version of plug-in.  
		/// </summary>
		/// <param name="guid">Plug-in identifier.</param>
		/// <returns>Version if plug-in is found, otherwise null.</returns>
		Version GetPluginVersion( Guid guid );
	}
}