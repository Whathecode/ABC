var _port;
const _keepAliveTimer = 1000;

// Array.find function.
if ( !Array.prototype.find )
{
	Object.defineProperty( Array.prototype, 'find',
	{
		enumerable: false,
		configurable: true,
		writable: true,
		value: function( predicate )
		{
			if ( this == null )
			{
				throw new TypeError( 'Array.prototype.find called on null or undefined' );
			}
			if ( typeof predicate !== 'function' )
			{
				new TypeError( 'predicate must be a function' );
			}
			var list = Object( this );
			var length = list.length >>> 0;
			var thisArg = arguments[ 1 ];
			var value;

			for ( var i = 0; i < length; ++i )
			{
				if ( i in list )
				{
					value = list[ i ];
					if ( predicate.call( thisArg, value, i, list ) )
					{
						return value;
					}
				}
			}
			
			return undefined;
		}
	} );
}

var initializePort = function()
{
	_port = chrome.runtime.connectNative( "abc.applications.chrome.nativehost" );
	_port.onDisconnect.addListener( onDisconnect );
	_port.onMessage.addListener( onMessage );
}

var onDisconnect = function()
{
	// Reconnect when disconnected.
	initializePort();
}

var onMessage = function( message )
{
	console.log( "Received: " + JSON.stringify( message ) );
	
	switch ( message.request )
	{
		case "suspend":
			var windowTitle = message.data;
			chrome.windows.getAll( {populate: true}, function( windows )
			{
				// Find the corresponding window.
				var matchingWindow = windows.find( function( window, windowIndex, windowsArray ) {
					var matchingTab = window.tabs.find( function( tab, tabIndex, tabsArray ) {
						return tab.highlighted && tab.title == windowTitle;
					} );
					
					return matchingTab !== undefined;
				} );
				
				// Find all tab URLs.
				var windowTabs = [];
				if ( matchingWindow !== undefined )
				{
					windowTabs = matchingWindow.tabs.map( function( tab ) {
						return {url: tab.url, isPinned: tab.pinned};
					} );
				}
				
				// Report results.
				_port.postMessage(
					{
						reply: "suspend",
						data: {
							openTabs: windowTabs
						}
					}
				);
				
				// Close window. Tabs are closed separately, otherwise upon restart the previous tabs are reopened.
				var tabIds = matchingWindow.tabs.map( function( tab ) { return tab.id; } );
				chrome.tabs.remove( tabIds );
			} );
			break;
		
		case "resume":
			// Retrieve URLs to open.
			var persistedWindow = message.data;
			var urls = persistedWindow.openTabs.map ( function( tab ) {
				return tab.url;
			} );
			
			// Open URLs and set tab properties.
			chrome.windows.create( {url: urls}, function( window ) {
				for ( i = 0; i < window.tabs.length; ++i )
				{
					if ( persistedWindow.openTabs[ i ].isPinned )
					{
						chrome.tabs.update( window.tabs[ i ].id, {pinned: true} );
					}
				}
			} );
			break;
	}
}


// Connect to native host.
initializePort();

// Start timer which sends keep alive signals to native host.
setInterval(
	function() {
		_port.postMessage( { request: "keepAlive" } );
	}, _keepAliveTimer
);