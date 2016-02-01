namespace ABC.Plugins.Manager
{
	/// <summary>
	/// Interface should be implemented by plug-ins that require additional custom installation steps.
	/// </summary>
	public interface IInstallable
	{
		PluginInfo PluginInfo { get; }


		/// <summary>
		/// Performs all actions that are necessary to make plug-in installed on user's system.
		/// </summary>
		/// <returns>True if success, false otherwise.</returns>
		bool Install( params object[] args );

		/// <summary>
		/// Performs all actions that are necessary to erase plug-in from user's system.
		/// </summary>
		/// <returns>True if success, false otherwise.</returns>
		bool Unistall( params object[] args );
	}
}