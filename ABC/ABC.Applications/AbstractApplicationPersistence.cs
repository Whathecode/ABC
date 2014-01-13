using System.Diagnostics;


namespace ABC.Applications
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
		public abstract void Suspend( Process toSuspend );
	}
}
