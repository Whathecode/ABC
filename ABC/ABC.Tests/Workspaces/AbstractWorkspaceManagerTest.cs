using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Whathecode.Microsoft.VisualStudio.TestTools.UnitTesting;
using Whathecode.System;


namespace ABC.Workspaces
{
	[TestClass]
	public class AbstractWorkspaceManagerTest
	{
		TestWorkspaceManager _manager;


		[TestInitialize]
		public void TestInitialize()
		{
			_manager = new TestWorkspaceManager();
		}


		[TestMethod]
		public void DisposedTest()
		{
			_manager.Dispose();

			// Can no longer call public methods.
			AssertHelper.ThrowsException<ObjectDisposedException>( _manager.Close );
			AssertHelper.ThrowsException<ObjectDisposedException>( () => _manager.Merge( new TestWorkspace(), new TestWorkspace() ) );
			AssertHelper.ThrowsException<ObjectDisposedException>( () => _manager.SwitchToWorkspace( new TestWorkspace() ) );
			AssertHelper.ThrowsException<ObjectDisposedException>( () => _manager.CreateEmptyWorkspace() );
			AssertHelper.ThrowsException<ObjectDisposedException>( () => _manager.CreateWorkspaceFromSession( new TestSession() ) );
		}

		[TestMethod]
		public void IncorrectInitializationTest()
		{
			var incorrect = new IncorrectWorkspaceManager();

			Assert.IsNull( incorrect.StartupWorkspace );
			Assert.IsNull( incorrect.CurrentWorkspace );
			AssertHelper.ThrowsException<InvalidImplementationException>( incorrect.Close );
			AssertHelper.ThrowsException<InvalidImplementationException>( () => incorrect.Merge( new TestWorkspace(), new TestWorkspace() ) );
			AssertHelper.ThrowsException<InvalidImplementationException>( () => incorrect.SwitchToWorkspace( new TestWorkspace() ) );
			AssertHelper.ThrowsException<InvalidImplementationException>( () => incorrect.CreateEmptyWorkspace() );
			AssertHelper.ThrowsException<InvalidImplementationException>( () => incorrect.CreateWorkspaceFromSession( new TestSession() ) );
		}

		[TestMethod]
		public void StartupDestopTest()
		{
			// The first active workspace is the startup workspace.
			TestWorkspace startup = _manager.StartupWorkspace;
			Assert.IsNotNull( startup );
			Assert.AreEqual( startup, _manager.CurrentWorkspace );

			// When switching workspaces, the startup workspace does not change.
			_manager.SwitchToWorkspace( _manager.CreateEmptyWorkspace() );
			Assert.AreEqual( startup, _manager.StartupWorkspace );
		}

		[TestMethod]
		public void CreateWorkspacesTest()
		{
			// At startup, a startup workspace is created.
			Assert.AreEqual( 1, _manager.Workspaces.Count );
			Assert.IsTrue( _manager.Workspaces.First().Equals( _manager.StartupWorkspace ) );

			// Any other workspaces are added after calling Create...().
			TestWorkspace empty = _manager.CreateEmptyWorkspace();
			Assert.AreEqual( 2, _manager.Workspaces.Count );
			Assert.IsTrue( _manager.Workspaces.Contains( empty ) );
			TestWorkspace fromSession = _manager.CreateWorkspaceFromSession( new TestSession() );
			Assert.AreEqual( 3, _manager.Workspaces.Count );
			Assert.IsTrue( _manager.Workspaces.Contains( fromSession ) );
		}

		[TestMethod]
		public void SwitchWorkspaceTest()
		{
			TestWorkspace from = _manager.StartupWorkspace;
			TestWorkspace other = _manager.CreateEmptyWorkspace();

			// Switch to current.
			_manager.SwitchToWorkspace( from );
			Assert.AreEqual( from, _manager.CurrentWorkspace );

			// Switch to other.
			_manager.SwitchToWorkspace( other );
			Assert.AreEqual( other, _manager.CurrentWorkspace );

			// Switching to a workspace not managed by this manager is impossible.
			AssertHelper.ThrowsException<ArgumentException>( () => _manager.SwitchToWorkspace( null ) );
			AssertHelper.ThrowsException<ArgumentException>( () => _manager.SwitchToWorkspace( new TestWorkspace() ) );
		}

		[TestMethod]
		public void MergeTest()
		{
			// ReSharper disable AccessToModifiedClosure

			// Normal merge. The origin workspace is removed.
			TestWorkspace from = _manager.CreateEmptyWorkspace();
			TestWorkspace target = _manager.CreateEmptyWorkspace();
			_manager.Merge( from, target );
			Assert.IsFalse( _manager.Workspaces.Contains( from ) );
			Assert.IsTrue( _manager.Workspaces.Contains( target ) );

			// Merge with itself, nothing should happen.
			_manager.Merge( target, target );

			// Attempting to merge workspaces not managed by this manager.
			AssertHelper.ThrowsException<ArgumentException>( () => _manager.Merge( new TestWorkspace(), target ) );
			AssertHelper.ThrowsException<ArgumentException>( () => _manager.Merge( null, target ) );
			AssertHelper.ThrowsException<ArgumentException>( () => _manager.Merge( target, new TestWorkspace() ) );
			AssertHelper.ThrowsException<ArgumentException>( () => _manager.Merge( target, null ) );
			AssertHelper.ThrowsException<ArgumentException>( () => _manager.Merge( null, null ) );

			// Can't merge startup workspace
			_manager = new TestWorkspaceManager(); // Previous exception disposed the manager.
			target = _manager.CreateEmptyWorkspace();
			AssertHelper.ThrowsException<ArgumentException>( () => _manager.Merge( _manager.StartupWorkspace, target ) );

			// Can't merge current desktop.
			_manager = new TestWorkspaceManager(); // Previous exception disposed the manager.
			TestWorkspace current = _manager.CreateEmptyWorkspace();
			target = _manager.CreateEmptyWorkspace();
			_manager.SwitchToWorkspace( current );
			AssertHelper.ThrowsException<ArgumentException>( () => _manager.Merge( current, target ) );

			// ReSharper restore AccessToModifiedClosure
		}

		[TestMethod]
		public void CloseTest()
		{
			_manager.CreateEmptyWorkspace();
			_manager.CreateEmptyWorkspace();
			_manager.Close();

			// All workspaces are merged with startup workspace when closing.
			Assert.AreEqual( 1, _manager.Workspaces.Count );
			Assert.AreEqual( _manager.StartupWorkspace, _manager.CurrentWorkspace );
			Assert.AreEqual( _manager.StartupWorkspace, _manager.Workspaces.First() );
		}
	}
}
