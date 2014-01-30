using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using ABC.Common;


namespace ABC.Applications.Persistence
{
	/// <summary>
	///   Provides persistence providers for supported applications by loading plugins from a specified folder.
	/// </summary>
	public class PersistenceProvider : AbstractPersistenceProvider
	{
		readonly CompositionContainer _pluginContainer;

		[ImportMany]
		readonly List<AbstractApplicationPersistence> _persistenceProviders = new List<AbstractApplicationPersistence>();


		public PersistenceProvider( string pluginFolderPath )
		{
			_pluginContainer = CompositionHelper.Compose( this, pluginFolderPath );
		}


		protected override List<AbstractApplicationPersistence> GetPersistenceProviders()
		{
			return _persistenceProviders;
		}
	}
}
