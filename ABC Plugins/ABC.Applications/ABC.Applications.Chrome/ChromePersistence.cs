using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using ABC.Applications.Persistence;
using Whathecode.System.Extensions;
using Whathecode.System.Windows;


namespace ABC.Applications.Chrome
{
	[Export( typeof( AbstractApplicationPersistence ) )]
	public class ChromePersistence : AbstractApplicationPersistence
	{
		readonly IpcClientChannel _channel;
		readonly IChromeAbcService _chromeService;


		public ChromePersistence()
			: base( "chrome" )
		{
			// Set up communication with the Chrome ABC extension.
			_channel = new IpcClientChannel();
			ChannelServices.RegisterChannel( _channel, false );
			_chromeService = (IChromeAbcService)Activator.GetObject( typeof( IChromeAbcService ), "ipc://ChromeAbcConnection/ChromeAbcService" );
		}


		public override object Suspend( SuspendInformation toSuspend )
		{
			var chromeWindows = toSuspend.Windows
				.Select( w => new WindowInfo( w.Handle ) )
				// For some reason, Chrome windows can use two separate classes. The same ones are also used for status bars, but status bars never have a title set.
				.Where( w => w.GetClassName().EqualsAny( "Chrome_WidgetWin_0", "Chrome_WidgetWin_1" ) && !w.GetTitle().EqualsAny( "", "Chrome App Launcher" ) );

			var persisted = new List<ChromePersistedWindow>();
			foreach ( var w in chromeWindows )
			{
				bool hasRetried = false;
				while ( true )
				{
					try
					{
						// TODO: The window title is passed in order to be able to identify which window needs to be suspended. This title can change dynamically however, so is there a safer way?
						string title = w.GetTitle();
						const string chromeSuffix = " - Google Chrome";
						if ( title.EndsWith( chromeSuffix ) )
						{
							title = title.Substring( 0, title.Length - chromeSuffix.Length );
						}
						persisted.Add( _chromeService.Suspend( title ) );
						break;
					}
					catch ( RemotingException )
					{
						// When a new server has started, and thus the previous pipe connection was closed, the first call results in a 'pipe is closing' exception.
						// A second call however can succeed, now that the connection is aware it was closed, and a reconnect is attempted.
						if ( hasRetried )
						{
							break;
						}

						hasRetried = true;
					}
				}
			}

			return persisted;
		}

		public override void Resume( string applicationPath, object persistedData )
		{
			// TODO: Chrome might not have started yet. Either launch chrome directly and then call the extension, or use command line parameters.
			// Command line parameters will always provide less flexibility for future implementations.

			var data = (List<ChromePersistedWindow>)persistedData;
			foreach ( var d in data )
			{
				_chromeService.Resume( d );
			}
		}

		public override Type GetPersistedDataType()
		{
			return typeof( List<ChromePersistedWindow> );
		}
	}
}
