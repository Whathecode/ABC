using System;
using System.Collections.Generic;


namespace ABC.Common
{
	public class PluginInformation
	{
		public PluginInformation( string processName, string companyName = "Unknown company", string author = "Unknown author", List<string> supportedVersions = null,  string displayName = null )
		{
			ProcessName = processName;
			CompanyName = companyName;
			Author = author;
			SupportedVersions = supportedVersions ?? new List<String> {"-"};
			DisplayName = displayName ?? processName;
		}


		/// <summary>
		///   The name of the process that is connected to the plug-in.
		/// </summary>
		public string ProcessName { get; private set; }

		/// <summary>
		///   The name as plug-in will be displayed in plug-in manager (specify if different than the process name).
		/// </summary>
		public string DisplayName { get; private set; }

		/// <summary>
		///   The company name which produces the application connected to the process.
		/// </summary>
		public string CompanyName { get; private set; }

		/// <summary>
		///   The author of Plug-in.
		/// </summary>
		public string Author { get; private set; }

		/// <summary>
		///  Supported versions of application by this plug-in.
		/// </summary>
		public List<string> SupportedVersions { get; private set; }
	}
}