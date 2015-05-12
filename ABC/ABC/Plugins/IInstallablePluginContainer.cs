using System;


namespace ABC.Plugins
{
	public interface IInstallablePluginContainer
	{
		/// <summary>
		/// Reloads container, discovers all newly added or deleted plug-ins.
		/// </summary>
		void Refresh();

		/// <summary>
		/// Get an installable plug-in from container.
		/// </summary>
		/// <param name="guid">Plug-in identifier.</param>
		/// <returns></returns>
		IInstallable GetInstallablePlugin( Guid guid );

		/// <summary>
		/// Path to plug-ins directory.
		/// </summary>
		string PluginFolderPath { get; }
		
		/// <summary>
		/// Gives a version of plug-in.  
		/// </summary>
		/// <param name="guid">Plug-in identifier.</param>
		/// <returns>Version if plug-in is found, otherwise null.</returns>
		Version GetPluginVersion( Guid guid );
	}
}