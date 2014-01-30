using System.Collections.Generic;


namespace ABC.Applications.Persistence
{
	/// <summary>
	///   A persistence provider which simply exposes the application persistence collection as an editable list which is empty by default.
	/// </summary>
	public class CollectionPersistenceProvider : AbstractPersistenceProvider
	{
		public readonly List<AbstractApplicationPersistence> PersistenceProviders = new List<AbstractApplicationPersistence>();


		protected override List<AbstractApplicationPersistence> GetPersistenceProviders()
		{
			return PersistenceProviders;
		}
	}
}
