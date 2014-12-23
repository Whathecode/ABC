namespace ABC
{
	/// <summary>
	///   A workspace which is managed by an <see cref = "AbstractWorkspaceManager{TWorkspace, TSession}" />.
	/// </summary>
	/// <typeparam name = "TSession">The type which is used to serialize workspaces as a session.</typeparam>
	public abstract class AbstractWorkspace<TSession>
	{
		public IWorkspace NonGeneric { get; private set; }


		protected AbstractWorkspace()
		{
			NonGeneric = new NonGenericWorkspace<TSession>( this );
		}


		public abstract TSession Store();
	}
}
