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


		/// <summary>
		///   Create a new persistence provider with application persistence plugins loaded from the specified path.
		/// </summary>
		/// <param name = "pluginFolderPath">The path where application persistence plugins are located.</param>
		public PersistenceProvider( string pluginFolderPath )
		{
			_pluginContainer = CompositionHelper.ComposeFromPath( this, pluginFolderPath );
		}


		protected override List<AbstractApplicationPersistence> GetPersistenceProviders()
		{
			return _persistenceProviders;
		}
	}
}
