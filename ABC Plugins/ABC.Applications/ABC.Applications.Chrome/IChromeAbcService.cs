namespace ABC.Applications.Chrome
{
	public interface IChromeAbcService
	{
		ChromePersistedWindow Suspend( string windowTitle );
		void Resume( ChromePersistedWindow window );
	}
}
