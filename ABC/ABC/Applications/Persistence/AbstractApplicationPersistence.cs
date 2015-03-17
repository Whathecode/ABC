using System;
using System.Reflection;
using ABC.Plugins;


namespace ABC.Applications.Persistence
{
	/// <summary>
	///   Provides a way to persist the state of an application.
	/// </summary>
	public abstract class AbstractApplicationPersistence
	{
		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="assembly">In order to identify plug-in, AssemblyTargetProcess, GUID, 
		/// AssemblyVersion have to be included as assembly attributes.</param>
		protected AbstractApplicationPersistence( Assembly assembly )
		{
			// TODO: Verify if all assembly info is included.
			AssemblyInfo = new AssemblyInfo( assembly );
		}

		/// <summary>
		/// Provides information about assembly and its requirements.
		/// </summary>
		public AssemblyInfo AssemblyInfo { get; private set; }

		/// <summary>
		///   Persists the current state of the application, and then suspends it.
		/// </summary>
		/// <param name = "toSuspend">Holds information about the process to suspend.</param>
		/// <returns>The object which holds the persisted data.</returns>
		public abstract object Suspend( SuspendInformation toSuspend );

		/// <summary>
		///   Resume an application with the passed persisted state.
		/// </summary>
		/// <param name = "applicationPath">The path to the application for which persisted data was stored.</param>
		/// <param name = "persistedData">The object which holds the persisted data.</param>
		public abstract void Resume( string applicationPath, object persistedData );

		/// <summary>
		///   Returns the type which holds the persisted data, which needs to be serializable.
		/// </summary>
		public abstract Type GetPersistedDataType();
	}
}