namespace ABC.Workspaces
{
	/// <summary>
	///   A workspace which is managed by a <see cref = "IWorkspaceManager" />.
	/// </summary>
	public interface IWorkspace
	{
		/// <summary>
		///   Determines whether the workspace is currently in a suspended state,
		///   meaning all its resources have been closed and are ready to be stored.
		/// </summary>
		bool IsSuspended { get; }

		/// <summary>
		///   Determine whether there are resources left that need suspension in case <see cref="Suspend" /> is called.
		/// </summary>
		bool HasResourcesToSuspend();

		/// <summary>
		///   Suspends this workspace asynchronously, waiting for all resources to be closed and ready to be stored.
		/// </summary>
		void Suspend();

		/// <summary>
		///   Suspend the workspace, regardless of whether resoures are still open.
		///   A workaround could be simply closing the resource.
		///   Whichever approach taken, <see cref="HasResourcesToSuspend" /> should return false.
		/// </summary>
		void ForceSuspend();

		/// <summary>
		///   Resumes suspended resources which are held within the workspace.
		/// </summary>
		void Resume();

		/// <summary>
		///   Serialize the current workspace to a structure, allowing to restore it at a later time.
		///   In case the workspace is suspended, suspended resources are stored for later resumption as well.
		/// </summary>
		/// <returns>A structure holding the relevant information for this workspace.</returns>
		object Store();
	}
}
