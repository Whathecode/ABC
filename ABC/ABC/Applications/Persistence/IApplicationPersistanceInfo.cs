using System.Collections.Generic;


namespace ABC.Applications.Persistence
{
	public interface IApplicationPersistanceInfo
	{
		/// <summary>
		///   The name of the process this persistence provider can persist.
		/// </summary>
		string ProcessName { get; }
		
		/// <summary>
		///   The company name which produces the application connected to the process.
		/// </summary>
		string CompanyName { get; }

		/// <summary>
		///   Version of the plug-in.
		/// </summary>
		string Version { get; }

		/// <summary>
		///  Supported versions of application by this plug-in.
		/// </summary>
		List<string> SupportedVersions { get; }

		IApplicationPersistanceInfo GiveInfo();
	}
}
