using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.WindowsAPICodePack.Shell;


namespace ABC.Workspaces.Library
{
	/// <summary>
	///   Represents a Windows Shell library.
	/// </summary>
	public class Library : AbstractWorkspace<StoredLibrary>
	{
		/// <summary>
		///   The extension of microsoft libraries.
		/// </summary>
		const string LibraryExtension = "library-ms";

		readonly string _name;

		List<string> _paths;
		/// <summary>
		///   The list of directories of which the contents are shown in the library.
		/// </summary>
		public IReadOnlyCollection<string> Paths { get { return _paths; } }


		public Library( string name )
		{
			_name = name;
			_paths = new List<string>();
		}

		public Library( string name, StoredLibrary session )
		{
			_name = name;
			_paths = session.Paths.ToList();
		}


		public void Open()
		{
			string folderName = Path.Combine( ShellLibrary.LibrariesKnownFolder.Path, _name );
			Process.Start( "explorer.exe", Path.ChangeExtension( folderName, LibraryExtension ) );
		}

		protected override void ShowInner()
		{
			// Update paths to only include existing folders.
			_paths = _paths.Where( Directory.Exists ).ToList();

			// Initialize the shell library.
			// Information about Shell Libraries: http://msdn.microsoft.com/en-us/library/windows/desktop/dd758094(v=vs.85).aspx
			using ( var activityContext = new ShellLibrary( _name, true ) )
			{
				// TODO: Optionally set the icon of the library to the icon of the activity? For now, just set it to the icon of the executing assembly.
				activityContext.IconResourceId = new IconReference( Assembly.GetEntryAssembly().Location, 0 );

				int retries = 5;
				var pathsToAdd = new List<string>();
				pathsToAdd.AddRange( Paths );
				COMException innerException = null;
				while ( pathsToAdd.Count > 0 && retries > 0 )
				{
					foreach ( string path in _paths )
					{
						try
						{
							activityContext.Add( path );
							pathsToAdd.Remove( path );
						}
						catch ( COMException e )
						{
							innerException = e;
							// TODO: How to handle/prevent the COMException which is sometimes thrown?
							// System.Runtime.InteropServices.COMException (0x80070497): Unable to remove the file to be replaced.
						}
						finally
						{
							--retries;
						}
					}
				}
				if ( pathsToAdd.Count > 0 )
				{
					throw new COMException( "Something went wrong while initializing the Activity Context library.", innerException );
				}
			}
		}

		protected override void HideInner()
		{
			UpdatePaths();
		}

		/// <summary>
		///   Save any new paths which might have been added by the user.
		/// </summary>
		internal void UpdatePaths()
		{
			// When the library is not 'visible', its paths are not accessible and can not be changed.
			if ( !IsVisible )
			{
				return;
			}

			using ( var activityContext = ShellLibrary.Load( _name, true ) )
			{
				_paths.Clear();
				foreach ( var folder in activityContext )
				{
					_paths.Add( folder.Path );
				}
			}
		}

		/// <summary>
		///   Sets new paths which need to be displayed in the Windows Shell Library.
		/// </summary>
		/// <param name="newPaths">The paths of which the files need to be displayed in the library.</param>
		public void SetPaths( IEnumerable<string> newPaths )
		{
			_paths = newPaths.ToList();

			if ( IsVisible )
			{
				ShowInner();
			}
		}

		public override bool HasResourcesToSuspend()
		{
			return false;
		}

		protected override void SuspendInner()
		{
			// Nothing to do.
		}

		protected override void ForceSuspendInner()
		{
			// Nothing to do, since HasResourcesToSuspend always returns false.
		}

		protected override void ResumeInner()
		{
			// Nothing to do.
		}

		protected override StoredLibrary StoreInner()
		{
			return new StoredLibrary( this );
		}
	}
}
