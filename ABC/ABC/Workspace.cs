using System;
using System.Collections.Generic;
using System.Linq;
using Whathecode.System.Collections.Generic;


namespace ABC
{
	/// <summary>
	///   Represents one workspace managed by <see cref="WorkspaceManager" />.
	/// </summary>
	public class Workspace : AbstractWorkspace<WorkspaceSession>
	{
		readonly TupleList<Type, IWorkspace> _workspaces;

		/// <summary>
		///   List of workspaces, along with the types of the workspace managers which manage them.
		/// </summary>
		public IEnumerable<Tuple<Type, IWorkspace>> Workspaces { get { return _workspaces; } }


		public Workspace( IEnumerable<Tuple<Type, IWorkspace>> workspaces )
		{
			_workspaces = new TupleList<Type, IWorkspace>( workspaces );
		}


		public override bool HasResourcesToSuspend()
		{
			return _workspaces.Any( w => w.Item2.HasResourcesToSuspend() );
		}

		protected override void SuspendInner()
		{
			_workspaces.ForEach( w => w.Item2.Suspend() );
		}

		protected override void ForceSuspendInner()
		{
			_workspaces.ForEach( w => w.Item2.ForceSuspend() );
		}

		protected override void ResumeInner()
		{
			_workspaces.ForEach( w => w.Item2.Resume() );
		}

		protected override WorkspaceSession StoreInner()
		{
			return new WorkspaceSession( this );
		}
	}
}
