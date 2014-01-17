using System.Diagnostics;


namespace ABC.Applications.Persistence
{
	/// <summary>
	///   Provides a way to persist the state of an application.
	/// </summary>
	public abstract class AbstractApplicationPersistence
	{
		/// <summary>
		///   The name of the process this persistence provider can persist.
		/// </summary>
		public string ProcessName { get; private set; }


		protected AbstractApplicationPersistence( string processName )
		{
			ProcessName = processName;
		}


		/// <summary>
		///   Persists the current state of the application, and then suspends it.
		/// </summary>
		/// <param name = "toSuspend">The process to suspend.</param>
		/// <param name= "commandLine">The command line call used to initialize the process.</param>
		/// <returns>The object which holds the persisted data.</returns>
		public abstract object Suspend( Process toSuspend, string commandLine );
	}
}
