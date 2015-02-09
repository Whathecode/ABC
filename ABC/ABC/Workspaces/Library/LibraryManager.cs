using System.Linq;


namespace ABC.Workspaces.Library
{
	/// <summary>
	///   An <see cref = "AbstractWorkspaceManager{TWorkspace, TSession}" />
	///   which allows creating a Windows Shell Library, and swapping out its contents.
	/// </summary>
	/// <author>Steven Jeuris</author>
	public class LibraryManager : AbstractWorkspaceManager<Library, StoredLibrary>
	{
		/// <summary>
		///   The name of the Windows Shell Library.
		/// </summary>
		public string Name { get; private set; }


		/// <summary>
		///   Create a new Windows Shell Library with a specified name, or when it already exists, manages the existing one.
		/// </summary>
		/// <param name="name"></param>
		public LibraryManager( string name )
		{
			Name = name;

			// Initialize startup workspace.
			var startupLibrary = new Library( Name );
			SetStartupWorkspace( startupLibrary );
		}


		protected override Library CreateEmptyWorkspaceInner()
		{
			return new Library( Name );
		}

		protected override Library CreateWorkspaceFromSessionInner( StoredLibrary session )
		{
			return new Library( Name, session );
		}

		protected override void SwitchToWorkspaceInner( Library workspace )
		{
			CurrentWorkspace.Hide();
			workspace.Show();
		}

		protected override void MergeInner( Library from, Library to )
		{
			from.UpdatePaths();
			to.SetPaths( from.Paths.Concat( to.Paths ) );
		}

		protected override void CloseInner()
		{
			CurrentWorkspace.UpdatePaths(); // Make sure newly added/removed paths are stored as well.

			// TODO: Remove the library.
		}

		protected override void FreeUnmanagedResources()
		{
			// Nothing to do.
		}
	}
}
