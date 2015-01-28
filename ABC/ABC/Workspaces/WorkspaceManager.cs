using System;
using System.Collections.Generic;
using System.Linq;
using Whathecode.System.Extensions;
using Whathecode.System.Linq;


namespace ABC.Workspaces
{
	/// <summary>
	///   Allows creating and switching between different workspaces.
	///   Workspaces are an aggregate of other workspace managers of e.g. virtual dekstops, desktop icons and dedicated folders.
	/// </summary>
	public class WorkspaceManager : AbstractWorkspaceManager<Workspace, WorkspaceSession>
	{
		readonly IReadOnlyCollection<IWorkspaceManager> _managers;


		/// <summary>
		///   Creates a new workspace manager which aggregates the passed workspace managers.
		/// </summary>
		/// <param name = "workspaceManagers">The list of workspace managers which needs to be aggregated.</param>
		public WorkspaceManager( IEnumerable<IWorkspaceManager> workspaceManagers )
		{
			_managers = workspaceManagers.ToList();

			SetStartupWorkspace( new Workspace( _managers.Select( w => Tuple.Create( w.GetType(), w.StartupWorkspace ) ) ) );
		}


		/// <summary>
		///   Create an empty workspace with no content assigned to it.
		/// </summary>
		/// <returns>An empty workspace.</returns>
		protected override Workspace CreateEmptyWorkspaceInner()
		{
			return new Workspace( _managers.Select( m => Tuple.Create( m.GetType(), m.CreateEmptyWorkspace() ) ) );
		}

		/// <summary>
		///   Creates a new workspace from a stored session.
		/// </summary>
		/// <param name = "session">The stored session.</param>
		/// <returns>The restored workspace.</returns>
		protected override Workspace CreateWorkspaceFromSessionInner( WorkspaceSession session )
		{
			var stored = _managers.Zip( session.StoredWorkspaces, (m, w) => new { Manager = m, Workspace = w } ).ToList();

			// Verify whether the stored session contains the same workspace managers.
			if ( session.StoredWorkspaces.Count != _managers.Count || stored.Any( s => s.Manager.GetType() != Type.GetType( s.Workspace.Item1 ) ) )
			{
				var ex = new ArgumentException(
					"The passed WorkspaceSession does not contain sessions which can be loaded by the workspace managers passed to this WorkspaceManager.",
					"session" );
				CloseAndThrow( ex );
			}

			return new Workspace( stored.Select(
				s => Tuple.Create( Type.GetType( s.Workspace.Item1 ), s.Manager.CreateWorkspaceFromSession( s.Workspace.Item2 ) )
			) );
		}

		/// <summary>
		///   Switch to the given workspace.
		/// </summary>
		protected override void SwitchToWorkspaceInner( Workspace workspace )
		{
			var toSwitch = _managers.Zip( workspace.Workspaces, ( m, w ) => new { Manager = m, Workspace = w } ).ToList();

			// Verify whether local managers correspond to the workspace managers.
			if ( _managers.Count != toSwitch.Count || toSwitch.Any( s => s.Manager.GetType() != s.Workspace.Item1 ) )
			{
				CloseAndThrow( new ArgumentException( "The requested workspace to switch to do not belong to this workspace manager." ) );
			}

			toSwitch.ForEach( s => s.Manager.SwitchToWorkspace( s.Workspace.Item2 ) );
		}

		/// <summary>
		///   Merges all content from one workspace with that from another.
		/// </summary>
		protected override void MergeInner( Workspace from, Workspace to )
		{
			var toMerge = _managers.Zip( from.Workspaces, to.Workspaces, (m, f, t) => new { Manager = m, From = f, To = t } ).ToList();

			// Verify whether workspace managers from both workspaces correspond to the local managers.
			if ( _managers.Count != toMerge.Count || toMerge.Any( t => !t.Manager.GetType().EqualsAll( t.From.Item1, t.To.Item1 ) ) )
			{
				CloseAndThrow( new ArgumentException( "The requested workspaces to be merged do not both belong to this workspace manager." ) );
			}

			toMerge.ForEach( m => m.Manager.Merge( m.From.Item2, m.To.Item2 ) );
		}

		public override List<Type> GetPersistedDataTypes()
		{
			return _managers.SelectMany( m => m.GetPersistedDataTypes() ).ToList();
		}

		/// <summary>
		///   Closes the workspace manager by restoring content from all workspaces as if they weren't separate workspaces.
		/// </summary>
		protected override void CloseAdditional()
		{
			// Nothing to do.
		}

		protected override void FreeUnmanagedResources()
		{
			// Nothing to do.
		}
	}
}
