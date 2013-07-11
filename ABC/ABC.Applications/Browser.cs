using System.Diagnostics;


namespace ABC.Applications
{
	/// <summary>
	///   Access browser operations, regardless of the default set browser.
	/// </summary>
	public class Browser : AbstractService
	{
		internal Browser( ServiceProvider provider )
			: base( provider ) {}


		public void OpenWebsite( string url )
		{
			Process.Start( url );
		}
	}
}