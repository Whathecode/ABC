using System;
using System.Collections.Generic;
using System.Linq;


namespace ABC.Workspaces
{
	/// <summary>
	///   Implements the non-generic <see cref="IWorkspaceManager" /> for a generic <see cref="AbstractWorkspaceManager{TWorkspace, TSession}" />.
	/// </summary>
	/// <typeparam name="TWorkspace">The type of workspace this workspace manager manages.</typeparam>
	/// <typeparam name="TSession">The type which is used to serialize this workspace as a session.</typeparam>
	class NonGenericWorkspaceManager<TWorkspace, TSession> : IWorkspaceManager
		where TWorkspace : AbstractWorkspace<TSession>
	{
		readonly AbstractWorkspaceManager<TWorkspace, TSession> _inner;
 
		public IWorkspace StartupWorkspace { get { return _inner.StartupWorkspace.NonGeneric; } }
		public IWorkspace CurrentWorkspace { get { return _inner.CurrentWorkspace.NonGeneric; } }
		public IReadOnlyCollection<IWorkspace> Workspaces { get { return _inner.Workspaces.Select( w => w.NonGeneric ).ToList(); } }


		public NonGenericWorkspaceManager( AbstractWorkspaceManager<TWorkspace, TSession> workspaceManager )
		{
			_inner = workspaceManager;
		}


		public IWorkspace CreateEmptyWorkspace()
		{
			return _inner.CreateEmptyWorkspace().NonGeneric;
		}

		public IWorkspace CreateWorkspaceFromSession( object session )
		{
			if ( session != null && !(session is TSession) )
			{
				string msg = String.Format( "Invalid stored session passed. Object of type \"{0}\" expected.", typeof( TSession ) );
				throw new ArgumentException( msg, "session" );
			}

			return _inner.CreateWorkspaceFromSession( (TSession)session ).NonGeneric;
		}

		public void SwitchToWorkspace( IWorkspace workspace )
		{
			var genericWorkspace = MakeGeneric( workspace );

			_inner.SwitchToWorkspace( genericWorkspace );
		}

		public Type GetSessionType()
		{
			return typeof( TSession );
		}

		public void Merge( IWorkspace from, IWorkspace to )
		{
			var fromGeneric = MakeGeneric( from );
			var toGeneric = MakeGeneric( to );

			_inner.Merge( fromGeneric, toGeneric );
		}

		public List<Type> GetPersistedDataTypes()
		{
			return _inner.GetPersistedDataTypes();
		}

		TWorkspace MakeGeneric( IWorkspace workspace )
		{
			var generic = workspace as NonGenericWorkspace<TSession>;
			if ( generic == null )
			{
				string msg = String.Format( "Invalid workspace passed. Expected workspace needs to be of type \"{0}\".", typeof( TWorkspace ) );
				throw new ArgumentException( msg );
			}

			return (TWorkspace)generic.Inner;
		}

		public void Close()
		{
			_inner.Close();
		}

		public void Dispose()
		{
			_inner.Dispose();
		}
	}
}
