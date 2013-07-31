using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;


namespace ABC.Applications
{
	/// <summary>
	///   Access browser operations, regardless of the default set browser.
	/// </summary>
	public class Browser : AbstractService
	{
		enum BrowserApplication
		{
			Unknown,
			Chrome,
			Firefox,
			InternetExplorer
		}


		BrowserApplication _browser = BrowserApplication.Unknown;
		readonly FileInfo _browserPath = null;


		internal Browser( ServiceProvider provider )
			: base( provider )
		{
			// Find default browser.
			string progId;
			using ( RegistryKey userChoiceKey = Registry.CurrentUser.OpenSubKey( @"Software\Microsoft\Windows\Shell\Associations\UrlAssociations\http\UserChoice" ) )
			{
				if ( userChoiceKey == null )
				{
					return;
				}
				object progIdValue = userChoiceKey.GetValue( "Progid" );
				if ( progIdValue == null )
				{
					return;
				}
				progId = progIdValue.ToString();
				switch ( progId )
				{
					case "IE.HTTP":
						_browser = BrowserApplication.InternetExplorer;
						break;
					case "FirefoxURL":
						_browser = BrowserApplication.Firefox;
						break;
					case "ChromeHTML":
						_browser = BrowserApplication.Chrome;
						break;
					default:
						_browser = BrowserApplication.Unknown;
						break;
				}
			}

			// Find browser path.
			const string exeSuffix = ".exe";
			using ( RegistryKey pathKey = Registry.ClassesRoot.OpenSubKey( progId + @"\shell\open\command" ) )
			{
				if ( pathKey == null )
				{
					return;
				}

				// Trim parameters.
				try
				{
					string path = pathKey.GetValue( null ).ToString().ToLower().Replace( "\"", "" );
					if ( !path.EndsWith( exeSuffix ) )
					{
						path = path.Substring( 0, path.LastIndexOf( exeSuffix, StringComparison.Ordinal ) + exeSuffix.Length );
						_browserPath = new FileInfo( path );
					}
				}
				// ReSharper disable EmptyGeneralCatchClause
				catch
				{
					// Assume the registry value is set incorrectly, or some funky browser is used which currently is unknown.
				}
				// ReSharper restore EmptyGeneralCatchClause
			}
		}


		public void OpenWebsite( string url )
		{
			if ( ServiceProvider.ForceOpenInNewWindow )
			{
				var startProcess = new ProcessStartInfo( _browserPath.FullName );
				switch ( _browser )
				{
					case BrowserApplication.Chrome:
						startProcess.Arguments = url + " --new-window";
						break;
					case BrowserApplication.Firefox:
						startProcess.Arguments = "-new-window " + url;
						break;
					case BrowserApplication.InternetExplorer:
						// IE always seems to open in new window, and it is even impossible to open as a tab in an existing instance?
						startProcess.Arguments = url;
						break;
					case BrowserApplication.Unknown:
						Process.Start( url );
						break;
				}

				Process.Start( startProcess );
			}
			else
			{
				// Just use the default windows behavior.
				Process.Start( url );
			}
		}
	}
}