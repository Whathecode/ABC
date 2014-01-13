using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.Linq;
using ABC.Common;


namespace ABC.Applications
{
	/// <summary>
	///   Provides persistence providers for supported applications.
	/// </summary>
	public class PersistenceProvider
	{
		readonly CompositionContainer _pluginContainer;

		[ImportMany]
		readonly List<AbstractApplicationPersistence> _persistenceProviders = new List<AbstractApplicationPersistence>();


		public PersistenceProvider( string pluginFolderPath )
		{
			_pluginContainer = CompositionHelper.Compose( this, pluginFolderPath );
		}


		public bool IsProviderAvailable( Process process )
		{
			// TODO: More unique identification of processes?
			return _persistenceProviders.Any( p => p.ProcessName == process.ProcessName );
		}

		public AbstractApplicationPersistence GetProvider( Process key )
		{
			return _persistenceProviders.First( p => p.ProcessName == key.ProcessName );
		}
	}
}
