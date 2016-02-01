using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Text.RegularExpressions;


namespace ABC.Plugins
{
	/// <summary>
	///   Specifies which process is targeted by a plugin.
	/// </summary>
	public class TargetProcess
	{
		/// <summary>
		///   The name of the executable of the targeted process (not including the extension).
		/// </summary>
		public string Name { get; }

		/// <summary>
		///   The company name of the targeted process, as specified in <see cref="FileVersionInfo" />.
		/// </summary>
		public string Company { get; }

		/// <summary>
		///   The expected version of the targeted process. When null, all versions are targeted (default).
		///   Version numbers do not need to be complete; 'underlying' versions are also targeted.
		/// </summary>
		public string Version { get; }


		/// <summary>
		/// 
		/// </summary>
		/// <param name = "name">The name of the executable of the targeted process (not including the extension).</param>
		/// <param name = "company">The company name of the targeted process, as specified in <see cref="FileVersionInfo" />.</param>
		/// <param name = "version">
		///   The expected version of the targeted process. When null, all versions are targeted (default).
		///   Version numbers do not need to be complete; 'underlying' versions are also targeted.
		/// </param>
		public TargetProcess( string name, string company, string version = null )
		{
			if ( version != null && !Regex.IsMatch( version, @"^(\d+\.){0,3}(\d+)$" ) )
			{
				throw new ArgumentException(
					"Version should be null to target all versions, or in the format of up to 4 dot separated digits, e.g., \"1.2\".",
					nameof( version ) );
			}

			Name = name;
			Company = company;
			Version = version;
		}

		/// <summary>
		///   Verifies whether the given process matches the targeted process.
		/// </summary>
		/// <param name="process">The process to verify whether it matches this targeted process.</param>
		/// <returns>True when the process matches the targeted process; false otherwise.</returns>
		public bool Matches( Process process )
		{
			FileVersionInfo info = process.MainModule.FileVersionInfo;
			Version fileVersion = new Version( info.FileMajorPart, info.FileMinorPart, info.FileBuildPart, info.FilePrivatePart );
			return
				Name == process.ProcessName &&
				Company == info.CompanyName &&
				( Version == null || fileVersion.Matches( Version ) ); // 'null' matches any version.	
		}
	}
}
