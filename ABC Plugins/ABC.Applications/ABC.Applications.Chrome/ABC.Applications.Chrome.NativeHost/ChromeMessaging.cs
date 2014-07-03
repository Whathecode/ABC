using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;


namespace ABC.Applications.Chrome.NativeHost
{
	/// <summary>
	///   Class used to handle messaging with a chrome plugin through standard input and output.
	/// </summary>
	class ChromeMessaging
	{
		readonly Stream _inputStream;
		readonly Stream _outputStream;

		public event Action<JObject> MessageReceived = delegate { }; 


		public ChromeMessaging( Stream input, Stream output )
		{
			_inputStream = input;
			_outputStream = output;

			Task.Factory.StartNew( AwaitInputMessage );
		}


		async void AwaitInputMessage()
		{
			byte[] lengthBuffer = await ReadBytesAsync( 4 );
			int length = BitConverter.ToInt32( lengthBuffer, 0 );
			
			byte[] messageBuffer = await ReadBytesAsync( length );
			JObject message = JObject.Parse( Encoding.UTF8.GetString( messageBuffer ) );
			MessageReceived( message );

			// ReSharper disable once CSharpWarnings::CS4014
			Task.Factory.StartNew( AwaitInputMessage );
		}

		async Task<byte[]> ReadBytesAsync( int amount )
		{
			byte[] readBytes = new byte[ amount ];
			int totalRead = 0;
			while ( totalRead < amount )
			{
				int read = await _inputStream.ReadAsync( readBytes, totalRead, amount - totalRead );
				totalRead += read;

				if ( read == 0 )
				{
					await Task.Delay( 100 );
				}
			}

			return readBytes;
		}

		public void WriteMessage( JObject message )
		{
			byte[] writeBytes =  Encoding.UTF8.GetBytes( message.ToString() );

			_outputStream.Write( BitConverter.GetBytes( writeBytes.Length ), 0, 4 );
			_outputStream.Write( writeBytes, 0, writeBytes.Length );
			_outputStream.Flush();
		}
	}
}
