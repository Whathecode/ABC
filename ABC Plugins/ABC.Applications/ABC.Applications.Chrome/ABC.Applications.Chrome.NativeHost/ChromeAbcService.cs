using System;
using System.Reactive.Linq;
using Newtonsoft.Json.Linq;


namespace ABC.Applications.Chrome.NativeHost
{
	class ChromeAbcService : MarshalByRefObject, IChromeAbcService
	{
		readonly ChromeMessaging _messaging;

		event Action<ChromePersistedWindow> SuspendReply = delegate { };  


		public ChromeAbcService( ChromeMessaging messaging )
		{
			_messaging = messaging;

			_messaging.MessageReceived += OnMessageReceived;
		}


		void OnMessageReceived( JObject message )
		{
			// Only listen to replies to requests.
			JToken reply;
			message.TryGetValue( "reply", out reply );
			if ( reply == null )
			{
				return;
			}

			// Trigger events for each of the replies.
			string replyTo = reply.Value<string>();
			switch ( replyTo )
			{
				case "suspend":
					JToken data = message.GetValue( "data" );
					SuspendReply( data.ToObject<ChromePersistedWindow>() );
					break;
			}
		}


		public ChromePersistedWindow Suspend( string windowTitle )
		{
			IObservable<ChromePersistedWindow> awaitReply = Observable.FromEvent<Action<ChromePersistedWindow>, ChromePersistedWindow>( ev => SuspendReply += ev, ev => SuspendReply -= ev ).Take( 1 );
			awaitReply.Subscribe();

			// Send suspend request. The window title is used to identify the window.
			JObject suspend = JObject.FromObject( new { request = "suspend", data = windowTitle } );
			_messaging.WriteMessage( suspend );

			// Await request.
			return awaitReply.Wait();
		}

		public void Resume( ChromePersistedWindow window )
		{
			JObject resume = JObject.FromObject( new { request = "resume", data = window } );
			_messaging.WriteMessage( resume );
		}
	}
}
