using System;
using System.Collections.Generic;


namespace ABC.Workspaces
{
	/// <summary>
	///   Allows creating and switching between different workspaces.
	/// </summary>
	public interface IWorkspaceManager : IDisposable
	{
		/// <summary>
		///   The workspace which is created when the workspace manager is first initialized.
		/// </summary>
		IWorkspace StartupWorkspace { get; }

		/// <summary>
		///   The currently active workspace, or null when none active.
		/// </summary>
		IWorkspace CurrentWorkspace { get; }

		/// <summary>
		///   A list of all workspaces managed by this workspace manager.
		/// </summary>
		IReadOnlyCollection<IWorkspace> Workspaces { get; }


		/// <summary>
		///   Create an empty workspace with no content assigned to it and adds it to the managed workspaces.
		/// </summary>
		/// <returns>An empty workspace.</returns>
		IWorkspace CreateEmptyWorkspace();

		/// <summary>
		///   Creates a new workspace from a stored session and adds it to the managed workspaces.
		/// </summary>
		/// <param name = "session">The stored session.</param>
		/// <returns>The restored workspace.</returns>
		IWorkspace CreateWorkspaceFromSession( object session );

		/// <summary>
		///   Switch to the given workspace, making it the current desktop.
		/// </summary>
		/// <param name="workspace">The workspace to switch to.</param>
		void SwitchToWorkspace( IWorkspace workspace );

		/// <summary>
		///   Merges all content from one workspace with that from another, and removes the original workspace.
		///   You can't merge the <see cref = "StartupWorkspace"/> with another workspace.
		/// </summary>
		/// <param name="from">The workspace which needs to be merged with <paramref name="to" />.</param>
		/// <param name="to"> The workspace to which <paramref name="from" /> is being merged.</param>
		void Merge( IWorkspace from, IWorkspace to );

		/// <summary>
		///   Returns the types which are used to store persisted data. This needs to be passed to DataContractSerializer when serializing.
		/// </summary>
		List<Type> GetPersistedDataTypes();

		/// <summary>
		///   Closes the workspace manager by restoring content from all workspaces as if they weren't separate workspaces.
		/// </summary>
		void Close();
	}
}
