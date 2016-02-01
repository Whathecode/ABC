using System;
using System.Diagnostics;
using ABC.Plugins;


namespace ABC.Applications.Persistence
{
	/// <summary>
	///   Provides a way to persist the state of an application.
	/// </summary>
	public abstract class AbstractApplicationPersistence : AbstractPlugin
	{
		/// <summary>
		///   The name of the process this persistence provider can persist.
		/// </summary>
		public string ProcessName { get; private set; }

		/// <summary>
		///   The name of the company that produced the application process, as specified in <see cref="FileVersionInfo" />.
		/// </summary>
		public string CompanyName { get; private set; }

		/// <summary>
		///   The version of the process for which this persistence provider works. When null, all versions are targeted by default.
        ///   Version numbers do not need to be complete; 'underlying' versions are also targeted.
		/// </summary>
		public string TargetVersion { get; private set; }


		/// <summary>
		///   Instantiate a new plugin which can persisted a specified application process.
		/// </summary>
		/// <param name = "processName">The name of the process this persistence provider can persist.</param>
		/// <param name = "companyName">The name of the company that produced the application process, as specified in <see cref="FileVersionInfo" />.</param>
		/// <param name = "targetVersion">
		///   The version of the process for which this persistence provider works. When null, all versions are targeted by default.
		///   Version numbers do not need to be complete; 'underlying' versions are also targeted.
		/// </param>
		protected AbstractApplicationPersistence( string processName, string companyName, string targetVersion = null )
		{
			ProcessName = processName;
			CompanyName = companyName;
			TargetVersion = targetVersion;
		}


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