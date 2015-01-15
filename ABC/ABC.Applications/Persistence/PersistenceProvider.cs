using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using ABC.Common;


namespace ABC.Applications.Persistence
{
	/// <summary>
	///   Provides persistence providers for supported applications by loading plugins from a specified folder.
	/// </summary>
	public class PersistenceProvider : AbstractPersistenceProvider, IInstallablePluginContainer
	{
		CompositionContainer _pluginContainer;
		readonly string _pluginFolderPath;

		[ImportMany]
		readonly List<AbstractApplicationPersistence> _persistenceProviders = new List<AbstractApplicationPersistence>();


		/// <summary>
		///   Create a new persistence provider with application persistence plugins loaded from the specified path.
		/// </summary>
		/// <param name = "pluginFolderPath">The path where application persistence plugins are located.</param>
		public PersistenceProvider( string pluginFolderPath )
		{
			_pluginFolderPath = pluginFolderPath;
			Reload();
		}

		public void Reload()
		{
			try
			{
				_pluginContainer = CompositionHelper.ComposeFromPath( this, _pluginFolderPath );
			}
			catch ( CompositionException )
			{
				Dispose();
				throw;
			}
		}

		protected override List<AbstractApplicationPersistence> GetPersistenceProviders()
		{
			return _persistenceProviders;
		}

		public List<PluginInformation> GetPluginInformation()
		{
			var infoCollection = new List<PluginInformation>();
			GetPersistenceProviders().ForEach( pp => infoCollection.Add( pp.Info ) );
			return infoCollection;
		}

		public IInstallable GetInstallablePlugin( string name, string companyName )
		{
			return GetPersistenceProviders()
				.FirstOrDefault( persistance => persistance.Info.ProcessName == name && persistance.Info.CompanyName == companyName ) as IInstallable;
		}
	}
}