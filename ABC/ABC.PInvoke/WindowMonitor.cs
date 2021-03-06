﻿using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Forms;
using Whathecode.System.Windows;
using WUser32 = Whathecode.Interop.User32;


namespace ABC.PInvoke
{
	public class WindowMonitor
	{
		#region Events

		public event ShellEvents.WindowCreatedEventHandler WindowCreated;
		public event ShellEvents.WindowDestroyedEventHandler WindowDestroyed;
		public event ShellEvents.WindowActivatedEventHandler WindowActivated;

		#endregion


		const int RshUnregister = 0;

		static int _wmShellhookmessage;
		static NativeWindowEx _hookWin;


		public void Start()
		{
			Task.Factory.StartNew( () =>
			{
				_hookWin = new NativeWindowEx();
				_hookWin.CreateHandle( new CreateParams() );

				if ( User32.RegisterShellHookWindow( _hookWin.Handle ) == false )
					throw new Exception( "Win32 error" );

				_wmShellhookmessage = (int)WUser32.RegisterWindowMessage( "SHELLHOOK" );
				_hookWin.MessageRecieved += ShellWinProc;
			} );
		}

		public void Stop()
		{
			//Shell32.RegisterShellHook( _hookWin.Handle, RshUnregister );
		}

		void ShellWinProc( ref Message m )
		{
			try
			{
				if ( m.Msg != _wmShellhookmessage ) return;
				switch ( (ShellMessages)m.WParam )
				{
					case ShellMessages.HSHELL_WINDOWCREATED:
						if ( WindowCreated != null )
						{
							WindowCreated( new WindowInfo( m.LParam ) );
						}
						break;
					case ShellMessages.HSHELL_WINDOWDESTROYED:
						if ( WindowDestroyed != null )
						{
							WindowDestroyed( m.LParam );
						}
						break;
					case ShellMessages.HSHELL_WINDOWACTIVATED:
						if ( WindowActivated != null )
						{
							WindowActivated( new WindowInfo( m.LParam ), false );
						}

						break;
					case ShellMessages.HSHELL_RUDEAPPACTIVATED:
						if ( WindowActivated != null )
						{
							WindowActivated( new WindowInfo( m.LParam ), false );
						}

						break;
				}
			}
			catch ( Exception ex )
			{
				Debug.WriteLine( ex );
			}
		}
	}
}