﻿using System;
using System.Linq;
using System.Runtime.Serialization;
using Whathecode.System.Collections.Generic;


namespace ABC
{
	/// <summary>
	///   Allows storing the state of a <see cref = "Workspace" /> at a particular point in time.
	/// </summary>
	[DataContract]
	public class WorkspaceSession
	{
		/// <summary>
		///   List of serialized workspaces, along with the types of the workspace managers which manage them.
		/// </summary>
		[DataMember]
		internal TupleList<Type, object> StoredWorkspaces;


		public WorkspaceSession( Workspace workspace )
		{
			StoredWorkspaces = new TupleList<Type, object>( workspace.Workspaces.Select( w => Tuple.Create( w.Item1, w.Item2.Store() ) ) );
		}
	}
}
