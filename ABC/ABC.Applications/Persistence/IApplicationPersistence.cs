using System.Diagnostics;


namespace ABC.Applications.Persistence
{
	/// <summary>
	///   Provides a way to persist the state of an application.
	/// </summary>
	/// <typeparam name = "TData">The type of the object which holds the persisted data.</typeparam>
	public interface IApplicationPersistence<out TData>
	{
		/// <summary>
		///   Persists the current state of the application, and then suspends it.
		/// </summary>
		/// <param name = "toSuspend">The process to suspend.</param>
		/// <param name= "commandLine">The command line call used to initialize the process.</param>
		/// <returns>A data object which holds the persisted data.</returns>
		TData Suspend( Process toSuspend, string commandLine );
	}
}
