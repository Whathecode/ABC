using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Timers;
using System.Windows;
using Newtonsoft.Json.Linq;


namespace ABC.Applications.Chrome.NativeHost
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{
		const int Timeout = 1000;
		readonly Timer _isConnectedTimer = new Timer( Timeout );
		bool _isAlive = true;

		readonly IpcServerChannel _channel;


		public MainWindow()
		{
			InitializeComponent();

#if DEBUG
			Visibility = Visibility.Visible;
#endif

			// Set up IPC server.
			try
			{
				_channel = new IpcServerChannel( "ChromeAbcConnection" );
			}
			catch ( RemotingException )
			{
				// A previous server channel is still running.
				Application.Current.Shutdown();

				return;
			}

			// Set up messaging with Chrome.
			WriteOutput( "Starting up standard input/output communication with Chrome." );
			var messaging = new ChromeMessaging( Console.OpenStandardInput(),  Console.OpenStandardOutput() );
			messaging.MessageReceived += OnMessageReceived;

			// Shut down application if no keep alive message has been received.
			_isConnectedTimer.Elapsed += ( sender, args ) =>
			{
				if ( !_isAlive )
				{
					Dispatcher.Invoke( () => Application.Current.Shutdown() );
				}

				_isAlive = false;
			};
			_isConnectedTimer.Start();

			// Setup up channel for the Chrome ABC plugin to communicate with.
			ChannelServices.RegisterChannel( _channel, false );
			var chromeService = new ChromeAbcService( messaging );
			RemotingServices.Marshal( chromeService, "ChromeAbcService" );
		}


		void OnMessageReceived( JObject message )
		{
			WriteOutput( "Message received: " + message );

			JToken request;
			message.TryGetValue( "request", out request );
			if ( request != null && request.Value<string>().Equals( "keepAlive" ) )
			{
				_isAlive = true;
			}
		}

		void WriteOutput( string s )
		{
			Dispatcher.Invoke( () => Output.Text += s + "\n" );
		}
	}
}
