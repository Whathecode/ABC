using System;
using ABC.Plugins;


namespace ABC.Applications.Persistence
{
	/// <summary>
	///   Provides a way to persist the state of an application.
	/// </summary>
	public abstract class AbstractApplicationPersistence
	{
		protected AbstractApplicationPersistence( PluginInformation info )
		{
			Info = info;
		}

		/// <summary>
		///   Plug-in detailed information.
		/// </summary>
		internal PluginInformation Info;

		/// <summary>
		///   Persists the current state of the application, and then suspends it.
		/// </summary>
		/// <param name = "toSuspend">Holds information about the process to suspend.</param>
		/// <returns>The object which holds the persisted data.</returns>
		abstract public object Suspend( SuspendInformation toSuspend );

		/// <summary>
		///   Resume an application with the passed persisted state.
		/// </summary>
		/// <param name = "applicationPath">The path to the application for which persisted data was stored.</param>
		/// <param name = "persistedData">The object which holds the persisted data.</param>
		abstract public void Resume( string applicationPath, object persistedData );

		/// <summary>
		///   Returns the type which holds the persisted data, which needs to be serializable.
		/// </summary>
		abstract public Type GetPersistedDataType();
	}
}
