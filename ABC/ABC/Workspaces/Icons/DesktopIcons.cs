using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Whathecode.System;
using Whathecode.System.Windows;
using Whathecode.System.Windows.Win32Controls;


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

		internal List<Point> Icons { get; private set; }


		static DesktopIcons()
		{
			StartupDesktopFolder = Environment.GetFolderPath( Environment.SpecialFolder.Desktop );
		}

		internal DesktopIcons()
		{
			Folder = StartupDesktopFolder;
			Icons = new List<Point>();
		}

		internal DesktopIcons( StoredIcons icons )
		{
			Folder = icons.Folder;
			Icons = icons.Icons;
		}


		ListView GetDesktopListView()
		{
			WindowInfo listViewWindow = WindowManager
				.GetDesktopWindow()
				.GetChildWindows()
				.First( w => w.GetClassName() == "SysListView32" && w.GetTitle() == "FolderView" );
			return new ListView( listViewWindow );
		}

		protected override void ShowInner()
		{
			// Change desktop path.
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

			// Move icons to original positions.
			ListView listView = GetDesktopListView();
			var itemCount = listView.GetItemCount();
			if ( itemCount == Icons.Count )
			{
				for ( var i = 0; i < itemCount; ++i )
				{
					var point = Icons[ i ];
					listView.SetItemPosition( i, point );
				}
			}
		}

		protected override void HideInner()
		{
			// Save icon positions.
			ListView listView = GetDesktopListView();
			var itemCount = listView.GetItemCount();
			Icons.Clear();
			for ( var i = 0; i < itemCount; ++i )
			{
				var point = listView.GetItemPosition( i );
				Icons.Add( point );
			}
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
