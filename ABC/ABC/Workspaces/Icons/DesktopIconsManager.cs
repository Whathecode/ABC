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
			// TODO: How to best handle this? This should probably be an option, since different ways of merging could be desired.
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
