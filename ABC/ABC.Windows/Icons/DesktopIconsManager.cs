namespace ABC.Windows.Icons
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
			return new DesktopIcons( StartupWorkspace );
		}

		protected override DesktopIcons CreateWorkspaceFromSessionInner( StoredIcons session )
		{
			return new DesktopIcons( session );
		}

		protected override void SwitchToWorkspaceInner( DesktopIcons workspace )
		{
			CurrentWorkspace.SaveIcons();
			workspace.ShowIcons();
			// TODO: The positions of the icons do not seem to be updated yet.
		}

		protected override void MergeInner( DesktopIcons from, DesktopIcons to )
		{
			// TODO: How to best handle this? This should probably be an option, since different ways of merging could be desired.
			// Be aware that this is also called when exiting the system! A flag might need to be passed to disambiguate between these two cases.
		}

		protected override void CloseAdditional()
		{
			// Nothing to do.
		}

		protected override void FreeUnmanagedResources()
		{
			// Nothing to do.
		}
	}
}
