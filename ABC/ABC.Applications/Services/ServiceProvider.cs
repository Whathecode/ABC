namespace ABC.Applications.Services
{
	/// <summary>
	///   Services exposed by various installed applications which can be accessed in an application-agnostic way.
	/// </summary>
	public class ServiceProvider
	{
		/// <summary>
		///   Specifies that whenever a document opens, it should attempt to do so in a new window.
		/// </summary>
		public bool ForceOpenInNewWindow { get; set; }

		public Browser Browser { get; private set; }


		public ServiceProvider()
		{
			Browser = new Browser( this );
		}
	}
}
