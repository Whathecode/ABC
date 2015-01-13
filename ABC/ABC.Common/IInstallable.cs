﻿
namespace ABC.Common
{
	/// <summary>
	/// Interface should be implemented by plug-ins that require additional custom installation steps.
	/// </summary>
	public interface IInstallable
	{
		/// <summary>
		/// Performs all actions that are necessary to make plug-in installed on user's system.
		/// </summary>
		/// <returns>True if success, false otherwise.</returns>
		bool Install();
		
		/// <summary>
		/// Performs all actions that are necessary to erase plug-in from user's system.
		/// </summary>
		/// <returns>True if success, false otherwise.</returns>
		bool Unistall();
	}
}