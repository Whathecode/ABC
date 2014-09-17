using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using ABC.Applications.Persistence;
using ABC.Common;
using SHDocVw;
using Whathecode.Interop;
using Whathecode.System.Extensions;


namespace ABC.Applications.Explorer
{
	public struct ExplorerLocation
	{
		public string LocationName { get; set; }
		public string LocationUrl { get; set; }
		public byte[] Pidl { get; set; }
	}


	[Export( typeof( AbstractApplicationPersistence ) )]
	public class ExplorerPersistence : AbstractApplicationPersistence
	{
		public ExplorerPersistence()
			: base( new PluginInformation("explorer", "Microsoft Corporation", "Steven Jeuris") ) {}


		public override object Suspend( SuspendInformation toSuspend )
		{
			var cabinetWindows = new ShellWindows()
				.Cast<InternetExplorer>()
				// TODO: Is there a safer way to guarantee that it is actually the explorer we expect it to be?
				// For some reason, the process CAN be both "Explorer", and "explorer".
				.Where( e => Path.GetFileNameWithoutExtension( e.FullName ).IfNotNull( p => p.ToLower() ) == "explorer" )
				.ToList();

			var suspendedExplorerWindows = new List<ExplorerLocation>();
			foreach ( IWindow window in toSuspend.Windows )
			{
				// Check whether the window is an explorer cabinet window. (file browser)
				InternetExplorer cabinetWindow = cabinetWindows.FirstOrDefault( e =>
				{
					try
					{
						return window.Handle.Equals( new IntPtr( e.HWND ) );
					}
					catch ( COMException )
					{
						// This exception is thrown when accessing 'HWND' for some windows.
						// TODO: Why is this the case, and do we ever need to handle those windows?
						return false;
					}
				} );
				if ( cabinetWindow != null )
				{
					var persistedData = new ExplorerLocation
					{
						LocationName = cabinetWindow.LocationName,
						LocationUrl = cabinetWindow.LocationURL,
						Pidl = cabinetWindow.GetPidl()
					};
					cabinetWindow.Quit();
					suspendedExplorerWindows.Add( persistedData );
				}

				// TODO: Support other explorer windows, e.g. property windows ...
			}

			return suspendedExplorerWindows;
		}

		public override void Resume( string applicationPath, object persistedData )
		{
			var locations = (List<ExplorerLocation>)persistedData;

			foreach ( ExplorerLocation location in locations )
			{
				// Copy the PIDL to unmanaged memory.
				IntPtr pidl = Marshal.AllocHGlobal( location.Pidl.Length );
				Marshal.Copy( location.Pidl, 0, pidl, location.Pidl.Length );

				// Open an explorer window with the passed PIDL.
				var executeInfo = Shell32.ShellExecuteInfo.ExecutePidl( pidl, User32.WindowState.ShowNoActivate );
				Shell32.ShellExecuteEx( ref executeInfo );

				Marshal.Release( pidl );
			}
		}

		public override Type GetPersistedDataType()
		{
			return typeof( List<ExplorerLocation> );
		}
	}
}
