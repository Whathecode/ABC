using System.IO;
using System.Runtime.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace ABC.Workspaces
{
	[TestClass]
	public class WorkspaceTest
	{
		IWorkspaceManager[] _innerManagers;
		WorkspaceManager _manager;


		[TestInitialize]
		public void Initialize()
		{
			_innerManagers = new[] { new TestWorkspaceManager().NonGeneric, new TestWorkspaceManager().NonGeneric };
			_manager = new WorkspaceManager( _innerManagers );
		}


		[TestMethod]
		public void StoreTest()
		{
			Workspace empty = _manager.CreateEmptyWorkspace();
			object stored = empty.Store();

			// Verify whether the object can be serialized.
			var serializer = new DataContractSerializer( stored.GetType(), new [] { typeof( TestSession ) } );
			var stream = new MemoryStream();
			serializer.WriteObject( stream, stored );

			// Verify whether stored sessions can be loaded again.
			stream.Flush();
			stream.Position = 0;
			var session = (WorkspaceSession)serializer.ReadObject( stream );
			Workspace reloaded = _manager.CreateWorkspaceFromSession( session );
		}
	}
}
