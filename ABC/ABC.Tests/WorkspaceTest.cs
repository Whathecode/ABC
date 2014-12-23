using System.IO;
using System.Runtime.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace ABC.Tests
{
	[TestClass]
	public class WorkspaceTest
	{
		[TestMethod]
		public void StoreTest()
		{
			var innerManagers = new[] { new TestWorkspaceManager().NonGeneric, new TestWorkspaceManager().NonGeneric };
			var manager = new WorkspaceManager( innerManagers );
			Workspace empty = manager.CreateEmptyWorkspace();
			object stored = empty.Store();

			// Verify whether the object can be serialized.
			var serializer = new DataContractSerializer( stored.GetType(), new [] { typeof( TestSession ) } );
			var stream = new MemoryStream();
			serializer.WriteObject( stream, stored );

			// Verify whether stored sessions can be loaded again.
			stream.Flush();
			stream.Position = 0;
			var session = (WorkspaceSession)serializer.ReadObject( stream );
			Workspace reloaded = manager.CreateWorkspaceFromSession( session );
		}
	}
}
