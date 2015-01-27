using System;
using System.Collections.Generic;
using ABC.PInvoke;


namespace ABC.Windows.Icons
{
	/// <summary>
	///   Represents a collection of icons displayed on the desktop.
	/// </summary>
	/// <author>Steven Jeuris</author>
	public class DesktopIcons : AbstractWorkspace<StoredIcons>
	{
		/// <summary>
		///   The path to the folder where the files to be displayed as icons on the desktop are located.
		///   The default path used is the default desktop folder.
		/// </summary>
		public string Folder { get; private set; }

		internal List<DesktopIcon> Icons { get; private set; }


		internal DesktopIcons()
		{
			Folder = Environment.GetFolderPath( Environment.SpecialFolder.Desktop );
			Icons = IconManager.SaveDestopIcons();
		}

		internal DesktopIcons( DesktopIcons other )
		{
			Folder = other.Folder;
			Icons = other.Icons;
		}

		internal DesktopIcons( StoredIcons icons )
		{
			Folder = icons.Folder;
			Icons = icons.Icons;
		}


		internal void SaveIcons()
		{
			Icons = IconManager.SaveDestopIcons();
		}

		internal void ShowIcons()
		{
			IconManager.ChangeDesktopFolder( Folder );
			IconManager.ArrangeDesktopIcons( Icons );
		}

		public void ChangeFolder( string folder )
		{
			Folder = folder;
			// TODO: What if the folder is changed while within the workspace? This should be taken care of here.
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
