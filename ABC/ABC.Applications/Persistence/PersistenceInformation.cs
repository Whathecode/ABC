using System.Collections.Generic;
using System.Diagnostics;
using ABC.Common;


namespace ABC.Applications.Persistence
{
	/// <summary>
	///   Information about a process that needs to be suspended.
	///   TODO: This is mainly a DTO so accessibilty doesn't really matter, but keep an eye on this whether it stays this way.
	/// </summary>
	public class SuspendInformation
	{
		/// <summary>
		///   The process to suspend.
		/// </summary>
		public Process Process { get; set; }

		/// <summary>
		///   The windows of the process which are to be suspended.
		/// </summary>
		public List<IWindow> Windows { get; set; }

		/// <summary>
		///   The command line call used to initialize the process.
		/// </summary>
		public string CommandLine { get; set; }
	}
}
