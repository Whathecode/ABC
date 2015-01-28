using System;
using System.Collections.Generic;
using System.Linq;
using Whathecode.System.Collections.Generic;
using Whathecode.System.Reflection;
using Whathecode.System.Reflection.Extensions;


namespace ABC.Workspaces
{
	/// <summary>
	///   Represents one workspace managed by <see cref="WorkspaceManager" />, containing the aggregate of workspaces managed by <see cref="WorkspaceManager" />'s internal managers.
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


		/// <summary>
		///   Retrieves the containing workspace of a given type.
		/// </summary>
		/// <typeparam name="TWorkspace">The type of the workspace to look for.</typeparam>
		/// <exception cref="InvalidOperationException">Thrown when a workspace of the requested type was not found.</exception>
		/// <exception cref="NotImplementedException">Thrown when multiple workspaces are found. No method to disambiguate between them has been implemented yet.</exception>
		public TWorkspace GetInnerWorkspace<TWorkspace>()
		{
			Type searchFor = typeof( TWorkspace );
			var matching = _workspaces
				.Select(
					w => w.Item2.GetType().IsOfGenericType( typeof( NonGenericWorkspace<> ) )
						? w.Item2.GetType().GetProperty( "Inner", ReflectionHelper.InstanceMembers ).GetValue( w.Item2 )
						: w.Item2 )
				.Where( w => w.GetType() == searchFor ).ToList();

			switch ( matching.Count )
			{
				case 0:
					string message = String.Format( "The requested inner workspace of type \"{0}\" was not found.", searchFor );
					throw new InvalidOperationException( message );
				case 1:
					return (TWorkspace)matching.First();
				default:
					throw new NotImplementedException( "Multiple matching workspaces were found, which is currently not supported." );

			}
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
