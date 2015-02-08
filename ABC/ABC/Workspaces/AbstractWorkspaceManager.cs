using System;
using System.Collections.Generic;
using System.Linq;
using Whathecode.System;


namespace ABC.Workspaces
{
	/// <summary>
	///   Allows creating and switching between different workspaces.
	/// </summary>
	/// <typeparam name = "TWorkspace">The type of workspace this workspace manager manages.</typeparam>
	/// <typeparam name = "TSession">The type which is used to serialize workspaces as a session.</typeparam>
	public abstract class AbstractWorkspaceManager<TWorkspace, TSession> : AbstractDisposable
		where TWorkspace : AbstractWorkspace<TSession>
	{
		/// <summary>
		///   Exposes this workspace manager as a non-generic <see cref = "IWorkspaceManager" />.
		/// </summary>
		public IWorkspaceManager NonGeneric { get; private set; }

		/// <summary>
		///   The workspace which is created when the workspace manager is first initialized.
		/// </summary>
		public TWorkspace StartupWorkspace { get; private set; }

		/// <summary>
		///   The currently active workspace, or null when none active.
		/// </summary>
		public TWorkspace CurrentWorkspace { get; private set; }

		readonly HashSet<TWorkspace> _workspaces = new HashSet<TWorkspace>();
		readonly List<TWorkspace> _orderedWorkspaces = new List<TWorkspace>(); 
		/// <summary>
		///   A list of all workspaces managed by this workspace manager.
		/// </summary>
		public IReadOnlyCollection<TWorkspace> Workspaces { get { return _orderedWorkspaces; } }


		protected AbstractWorkspaceManager()
		{
			NonGeneric = new NonGenericWorkspaceManager<TWorkspace, TSession>( this );
		}


		/// <summary>
		///   Sets the passed workspace as the initial and current workspace, indicating it is the currently visible one.
		///   This needs to be called from the constructor in derived types in order to correctly initialize this class.
		/// </summary>
		/// <param name = "workspace">The workspace to be used as <see cref = "StartupWorkspace" />.</param>
		protected void SetStartupWorkspace( TWorkspace workspace )
		{
			if ( StartupWorkspace != null )
			{
				string msg = String.Format( "\"SetStartupWorkspace\" should only be called once in {0}.", GetType() );
				throw new InvalidImplementationException( msg );
			}

			StartupWorkspace = workspace;
			CurrentWorkspace = StartupWorkspace;
			_orderedWorkspaces.Add( workspace );
			_workspaces.Add( workspace );

			// The startup workspace is the one visible at startup.
			workspace.Show();
		}

		/// <summary>
		///   Verifies whether a public method call can be called. The workspace manager needs to be properly initialized, and not yet disposed.
		/// </summary>
		void VerifyValidState()
		{
			// Not yet disposed.
			ThrowExceptionIfDisposed();

			// Properly initialized.
			if ( StartupWorkspace == null )
			{
				string msg = String.Format( "\"SetStartupWorkspace\" isn't called in {0} and needs to be called in the constructor of deriving types.", GetType() );
				throw new InvalidImplementationException( msg );
			}
		}


		/// <summary>
		///   Create an empty workspace with no content assigned to it and adds it to the managed workspaces.
		/// </summary>
		/// <returns>An empty workspace.</returns>
		public TWorkspace CreateEmptyWorkspace()
		{
			VerifyValidState();

			TWorkspace workspace = CreateEmptyWorkspaceInner();
			_orderedWorkspaces.Add( workspace );
			_workspaces.Add( workspace );

			return workspace;
		}

		/// <summary>
		///   Create an empty workspace with no content assigned to it.
		/// </summary>
		/// <returns>An empty workspace.</returns>
		protected abstract TWorkspace CreateEmptyWorkspaceInner();

		/// <summary>
		///   Creates a new workspace from a stored session and adds it to the managed workspaces.
		/// </summary>
		/// <param name = "session">The stored session.</param>
		/// <returns>The restored workspace.</returns>
		public TWorkspace CreateWorkspaceFromSession( TSession session )
		{
			VerifyValidState();

			TWorkspace workspace = CreateWorkspaceFromSessionInner( session );
			_orderedWorkspaces.Add( workspace );
			_workspaces.Add( workspace );

			return workspace;
		}

		/// <summary>
		///   Creates a new workspace from a stored session.
		/// </summary>
		/// <param name = "session">The stored session.</param>
		/// <returns>The restored workspace.</returns>
		protected abstract TWorkspace CreateWorkspaceFromSessionInner( TSession session );

		/// <summary>
		///   Switch to the given workspace, making it the current desktop.
		/// </summary>
		/// <param name = "workspace">The workspace to switch to.</param>
		public void SwitchToWorkspace( TWorkspace workspace )
		{
			VerifyValidState();

			if ( !_workspaces.Contains( workspace ) )
			{
				CloseAndThrow( new ArgumentException( "The passed workspace is not managed by this workspace manager.", "workspace" ) );
			}

			if ( CurrentWorkspace == workspace )
			{
				return;
			}

			SwitchToWorkspaceInner( workspace );

			CurrentWorkspace = workspace;
		}

		/// <summary>
		///   Switch to the given workspace.
		/// </summary>
		protected abstract void SwitchToWorkspaceInner( TWorkspace workspace );

		/// <summary>
		///   Merges all content from one workspace with that from another, and removes the original workspace.
		///   You can't merge the <see cref = "StartupWorkspace"/> with another workspace, nor can you remove the current desktop.
		/// </summary>
		public void Merge( TWorkspace from, TWorkspace to )
		{
			VerifyValidState();

			if ( !_workspaces.Contains( from ) || !_workspaces.Contains( to ) )
			{
				CloseAndThrow( new ArgumentException( "The passed workspaces need to be managed by this workspace manager." ) );
			}

			if ( from == to )
			{
				// Merging a workspace with itself means nothing needs to be done.
				return;
			}

			if ( from == StartupWorkspace )
			{
				CloseAndThrow( new ArgumentException( "Can't remove the startup desktop.", "from" ) );
			}
			if ( from == CurrentWorkspace )
			{
				CloseAndThrow( new ArgumentException( "The passed desktop can't be removed since it's the current desktop.", "from" ) );
			}

			MergeInner( from, to );
			_orderedWorkspaces.Remove( from );
			_workspaces.Remove( from );
		}

		/// <summary>
		///   Merges all content from one workspace with that from another.
		/// </summary>
		/// <param name="from">The workspace which needs to be merged with <paramref name="to" />.</param>
		/// <param name="to"> The workspace to which <paramref name="from" /> is being merged.</param>
		protected abstract void MergeInner( TWorkspace from, TWorkspace to );

		/// <summary>
		///   Returns the types which are used to store persisted data. This needs to be passed to DataContractSerializer when serializing.
		/// </summary>
		public virtual List<Type> GetPersistedDataTypes()
		{
			return new [] { typeof( TSession ) }.ToList();
		}

		/// <summary>
		///   Closes the workspace manager by restoring content from all workspaces as if they weren't separate workspaces.
		/// </summary>
		public void Close()
		{
			VerifyValidState();

			SwitchToWorkspace( StartupWorkspace );
			CloseInner();
		}

		/// <summary>
		///   After having switched to the startup workspace, this is called to allow closing all workspaces as if they weren't separate workspaces.
		/// </summary>
		protected abstract void CloseInner();

		/// <summary>
		///   Throw an exception, but close the workspace first so that no content is lost.
		/// </summary>
		/// <param name = "exception">The exception to throw.</param>
		protected void CloseAndThrow( Exception exception )
		{
			Close();
			throw exception;
		}

		protected override void FreeManagedResources()
		{
			Close();
		}
	}
}
