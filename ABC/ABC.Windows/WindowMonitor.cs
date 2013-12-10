using System;
using System.Diagnostics;
using System.Windows.Forms;
using ABC.PInvoke;
using System.Threading.Tasks;


namespace ABC.Windows
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

				_wmShellhookmessage = User32.RegisterWindowMessage( "SHELLHOOK" );
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
							WindowCreated( new Window( m.LParam ) );
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
							WindowActivated( new Window( m.LParam ), false );
						}

						break;
					case ShellMessages.HSHELL_RUDEAPPACTIVATED:
						if ( WindowActivated != null )
						{
							WindowActivated( new Window( m.LParam ), false );
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