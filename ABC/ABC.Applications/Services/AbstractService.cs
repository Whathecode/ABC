namespace ABC.Applications.Services
{
	public abstract class AbstractService
	{
		protected ServiceProvider ServiceProvider { get; private set; }


		protected AbstractService( ServiceProvider provider )
		{
			ServiceProvider = provider;
		}
	}
}
