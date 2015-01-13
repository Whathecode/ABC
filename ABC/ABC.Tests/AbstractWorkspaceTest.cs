using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace ABC.Tests
{
	[TestClass]
	public class AbstractWorkspaceTest
	{
		TestWorkspace _workspace;


		[TestInitialize]
		public void Initialize()
		{
			_workspace = new TestWorkspace();
		}


		[TestMethod]
		public void SuspendTest()
		{
			Assert.IsFalse( _workspace.IsSuspended );
			_workspace.SuspendedWorkspace += workspace => Assert.IsTrue( _workspace.IsSuspended );
			_workspace.Suspend();

			// Awaiting suspension happens on a separate thread.
			while ( !_workspace.IsSuspended ) {} // Await suspension.
		}

		[TestMethod]
		public void ForceSuspendTest()
		{
			bool eventCalled = false;

			Assert.IsFalse( _workspace.IsSuspended );
			_workspace.SuspendedWorkspace += workspace => Assert.IsTrue( _workspace.IsSuspended );
			_workspace.ForceSuspendRequested += workspace => eventCalled = true;
			_workspace.ForceSuspend();
			Assert.IsTrue( eventCalled );

			// Awaiting suspension happens on a separate thread.
			while ( !_workspace.IsSuspended ) {} // Await suspension.

			// Calling Suspend() when already suspended should not fail.
			_workspace.Suspend();
		}

		[TestMethod]
		public void ResumeTest()
		{
			_workspace.Suspend();
			while ( !_workspace.IsSuspended ) {} // Await suspension.

			// Resume unsuspends.
			Assert.IsTrue( _workspace.IsSuspended );
			_workspace.Resume();
			Assert.IsFalse( _workspace.IsSuspended );

			// Calling Resume when already resumed should not fail.
			_workspace.Resume();
		}
	}
}
