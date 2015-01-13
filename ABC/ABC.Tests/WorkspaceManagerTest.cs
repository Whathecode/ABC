using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Whathecode.System.Extensions;


namespace ABC.Tests
{
	[TestClass]
	public class WorkspaceManagerTest
	{
		WorkspaceManager _manager;
		IWorkspaceManager[] _innerManagers;


		[TestInitialize]
		public void Initialize()
		{
			_innerManagers = new[] { new TestWorkspaceManager().NonGeneric, new TestWorkspaceManager().NonGeneric };
			_manager = new WorkspaceManager( _innerManagers );
		}


		[TestMethod]
		public void CreateWorkspacesTest()
		{
			// The inner workspaces should be the same amount than the passed workspace managers.
			int managerCount = _innerManagers.Length;

			// At startup, a startup workspace is created.
			Assert.AreEqual( managerCount, _manager.StartupWorkspace.Workspaces.Count() );

			// Add empty workspaces.
			Workspace empty = _manager.CreateEmptyWorkspace();
			Assert.AreEqual( managerCount, empty.Workspaces.Count() );
		}

		[TestMethod]
		public void CreateWorkspaceFromSessionTest()
		{
			Workspace workspace = _manager.CreateEmptyWorkspace();
			var session = (WorkspaceSession)workspace.Store();

			Workspace restored = _manager.CreateWorkspaceFromSession( session );
			Assert.AreEqual( _innerManagers.Length, restored.Workspaces.Count() );
		}

		[TestMethod]
		public void SwitchToWorkspaceTest()
		{
			Workspace workspace = _manager.CreateEmptyWorkspace();
			_manager.SwitchToWorkspace( workspace );

			// Verify whether all inner managers have also switched workspace.
			Assert.AreEqual( workspace, _manager.CurrentWorkspace );
			_innerManagers
				.Zip( workspace.Workspaces, (m, w) => new { Manager = m, Workspace = w } )
				.ForEach( p => Assert.AreEqual( p.Workspace.Item2, p.Manager.CurrentWorkspace ) );
		}

		[TestMethod]
		public void MergeTest()
		{
			Workspace workspace = _manager.CreateEmptyWorkspace();
			_manager.Merge( workspace, _manager.StartupWorkspace );

			// Verify whether all inner managers have also merged the workspace.
			_innerManagers.ForEach( m =>
			{
				Assert.AreEqual( 1, m.Workspaces.Count );
				Assert.IsTrue( m.Workspaces.Contains( m.StartupWorkspace ) ); // Only startup workspace remains.
			} );
		}
	}
}
