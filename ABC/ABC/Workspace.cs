using System;
using System.Collections.Generic;
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


		public override WorkspaceSession Store()
		{
			return new WorkspaceSession( this );
		}
	}
}
