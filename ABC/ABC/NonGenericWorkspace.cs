namespace ABC
{
	/// <summary>
	///   Implements the non-generic <see cref="IWorkspace" /> for a generic <see cref="AbstractWorkspace{TSession}" />.
	/// </summary>
	/// <typeparam name="TSession">The type which is used to serialize this workspace as a session.</typeparam>
	class NonGenericWorkspace<TSession> : IWorkspace
	{
		internal AbstractWorkspace<TSession> Inner { get; private set; }
 

		public NonGenericWorkspace( AbstractWorkspace<TSession> workspace )
		{
			Inner = workspace;
		}


		public object Store()
		{
			return Inner.Store();
		}
	}
}
