using System.IO;
using IWshRuntimeLibrary;


namespace ABC.Workspaces.Icons
{
	/// <summary>
	///   An <see cref = "AbstractWorkspaceManager{TWorkspace, TSession}" />
	///   which allows creating and switching between different collections of icons positioned on the desktop.
	/// </summary>
	/// <author>Steven Jeuris</author>
	public class DesktopIconsManager : AbstractWorkspaceManager<DesktopIcons, StoredIcons>
	{
		public DesktopIconsManager()
		{
			// Initialize currently visible desktop icons.
			var startupIcons = new DesktopIcons();
			SetStartupWorkspace( startupIcons );
		}


		protected override DesktopIcons CreateEmptyWorkspaceInner()
		{
			return new DesktopIcons();
		}

		protected override DesktopIcons CreateWorkspaceFromSessionInner( StoredIcons session )
		{
			return new DesktopIcons( session );
		}

		protected override void SwitchToWorkspaceInner( DesktopIcons workspace )
		{
			CurrentWorkspace.Hide();
			workspace.Show();
		}

		protected override void MergeInner( DesktopIcons from, DesktopIcons to )
		{
			// TODO: Are there different/better ways of handling this? Perhaps this should be an option?
			// Add a shortcut to the desktop folder being merged.
			var shell = new WshShell();
			string shortcutName = new DirectoryInfo( from.Folder ).Name + ".lnk";
			IWshShortcut shortcut = shell.CreateShortcut( Path.Combine( to.Folder, shortcutName ) );
			shortcut.TargetPath = from.Folder;
			shortcut.Save();
		}

		protected override void CloseInner()
		{
			// Nothing to do.
		}

		protected override void FreeUnmanagedResources()
		{
			CloseInner();
		}
	}
}
