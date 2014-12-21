﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Whathecode.System;


namespace ABC
{
	/// <summary>
	///   Allows creating and switching between different workspaces.
	/// </summary>
	/// <typeparam name = "TWorkspace">The type of workspace this workspace manager manages.</typeparam>
	/// <typeparam name = "TSession">The type which is used to serialize workspaces as a session.</typeparam>
	public abstract class AbstractWorkspaceManager<TWorkspace, TSession> : AbstractDisposable
		where TWorkspace : class, IWorkspace
	{
		/// <summary>
		///   The workspace which is created when the workspace manager is first initialized.
		/// </summary>
		public TWorkspace StartupWorkspace { get; private set; }

		/// <summary>
		///   The currently active workspace, or null when none active.
		/// </summary>
		public TWorkspace CurrentWorkspace { get; private set; }

		readonly List<TWorkspace> _workspaces = new List<TWorkspace>(); 
		/// <summary>
		///   A list of all workspaces managed by this workspace manager.
		/// </summary>
		public IReadOnlyCollection<TWorkspace> Workspaces { get { return _workspaces; } }


		/// <summary>
		///   Sets the passed workspace as the initial and current workspace, indicating it is the currently visible one.
		///   This needs to be called from the constructor in derived types in order to correctly initialize this class.
		/// </summary>
		/// <param name="workspace">The workspace to be used as <see cref="StartupWorkspace" />.</param>
		protected void SetStartupWorkspace( TWorkspace workspace )
		{
			StartupWorkspace = workspace;
			CurrentWorkspace = StartupWorkspace;
			_workspaces.Add( workspace );
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
				string msg = String.Format( "\"SetStartupWorkspace\" isn't called in {0} and needs to be called in deriving types.", GetType() );
				throw new InvalidOperationException( msg );
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
		/// <param name="workspace">The workspace to switch to.</param>
		public void SwitchToWorkspace( TWorkspace workspace )
		{
			VerifyValidState();

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
			Contract.Requires( from != null && to != null && from != to );

			VerifyValidState();

			if ( from == StartupWorkspace )
			{
				CloseAndThrow( new ArgumentException( "Can't remove the startup desktop.", "from" ) );
			}
			if ( from == CurrentWorkspace )
			{
				CloseAndThrow( new ArgumentException( "The passed desktop can't be removed since it's the current desktop.", "from" ) );
			}

			MergeInner( from, to );
			_workspaces.Remove( from );
		}

		/// <summary>
		///   Merges all content from one workspace with that from another.
		/// </summary>
		protected abstract void MergeInner( TWorkspace from, TWorkspace to );

		/// <summary>
		///   Closes the workspace manager by restoring content from all workspaces as if they weren't separate workspaces.
		/// </summary>
		public void Close()
		{
			VerifyValidState();

			CloseInner();
		}

		/// <summary>
		///   Closes the workspace manager by restoring content from all workspaces as if they weren't separate workspaces.
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
