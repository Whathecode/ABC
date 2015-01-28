namespace ABC.Workspaces
{
	/// <summary>
	///   Implements the non-generic <see cref="IWorkspace" /> for a generic <see cref="AbstractWorkspace{TSession}" />.
	/// </summary>
	/// <typeparam name="TSession">The type which is used to serialize this workspace as a session.</typeparam>
	class NonGenericWorkspace<TSession> : IWorkspace
	{
		internal AbstractWorkspace<TSession> Inner { get; private set; }

		public bool IsSuspended
		{
			get { return Inner.IsSuspended; }
		}
 

		public NonGenericWorkspace( AbstractWorkspace<TSession> workspace )
		{
			Inner = workspace;
		}


		public bool HasResourcesToSuspend()
		{
			return Inner.HasResourcesToSuspend();
		}

		public void Suspend()
		{
			Inner.Suspend();
		}

		public void ForceSuspend()
		{
			Inner.ForceSuspend();
		}

		public void Resume()
		{
			Inner.Resume();
		}

		public object Store()
		{
			return Inner.Store();
		}
	}
}
