namespace ABC.Applications
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
