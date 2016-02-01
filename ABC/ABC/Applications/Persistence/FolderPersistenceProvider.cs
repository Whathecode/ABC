using System.Collections.Generic;
using System.ComponentModel.Composition;
using ABC.Plugins;


namespace ABC.Applications.Persistence
{
	/// <summary>
	///   Provides persistence providers for supported applications by loading plugins from a specified folder.
	/// </summary>
	public class FolderPersistenceProvider : AbstractPersistenceProvider
	{
		readonly FolderPluginComposition _pluginContainer;

		[ImportMany]
		readonly List<AbstractApplicationPersistence> _persistenceProviders = new List<AbstractApplicationPersistence>();


		/// <summary>
		///   Create a new persistence provider with application persistence plugins loaded from the specified path.
		/// </summary>
		/// <param name = "pluginFolderPath">The path where application persistence plugins are located.</param>
		public FolderPersistenceProvider( string pluginFolderPath )
		{
			_pluginContainer = new FolderPluginComposition( this, pluginFolderPath );
		}


		protected override List<AbstractApplicationPersistence> GetPersistenceProviders()
		{
			return _persistenceProviders;
		}

		protected override void FreeManagedResources()
		{
			base.FreeManagedResources();

			_pluginContainer.Dispose();
		}
	}
}