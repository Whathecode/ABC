using System;
using System.Collections.Generic;
using System.IO;
using ABC.PInvoke;
using Whathecode.System;
using Whathecode.System.Windows;


namespace ABC.Workspaces.Icons
{
	/// <summary>
	///   Represents a collection of icons displayed on the desktop.
	/// </summary>
	/// <author>Steven Jeuris</author>
	public class DesktopIcons : AbstractWorkspace<StoredIcons>
	{
		static readonly string StartupDesktopFolder;

		/// <summary>
		///   The path to the folder where the files to be displayed as icons on the desktop are located.
		///   Default is the original startup desktop folder. When a path becomes invalid, as a fallback <see cref="Folder" /> will be changed to the original startup folder as well.
		/// </summary>
		public string Folder { get; private set; }

		internal List<DesktopIcon> Icons { get; private set; }


		static DesktopIcons()
		{
			StartupDesktopFolder = Environment.GetFolderPath( Environment.SpecialFolder.Desktop );
		}

		internal DesktopIcons()
		{
			Folder = StartupDesktopFolder;
			Icons = new List<DesktopIcon>();
		}

		internal DesktopIcons( StoredIcons icons )
		{
			Folder = icons.Folder;
			Icons = icons.Icons;
		}


		protected override void ShowInner()
		{
			try
			{
				EnvironmentHelper.SetFolderPath( Environment.SpecialFolder.Desktop, Folder );
			}
			catch ( ArgumentException )
			{
				// Invalid path. Retry with startup desktop folder.
				Folder = StartupDesktopFolder;
				ShowInner();
				return;
			}

			WindowManager.RefreshDesktop();
			IconManager.ArrangeDesktopIcons( Icons );
		}

		protected override void HideInner()
		{
			Icons = IconManager.SaveDestopIcons();
		}

		public void ChangeFolder( string folder )
		{
			if ( folder == Folder )
			{
				return;
			}

			Folder = folder;

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

		protected override StoredIcons StoreInner()
		{
			return new StoredIcons( this );
		}
	}
}
