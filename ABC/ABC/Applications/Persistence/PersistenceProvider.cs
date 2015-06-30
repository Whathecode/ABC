using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using ABC.Plugins;


namespace ABC.Applications.Persistence
{
	/// <summary>
	///   Provides persistence providers for supported applications by loading plugins from a specified folder.
	/// </summary>
	public class PersistenceProvider : AbstractPersistenceProvider, IInstallablePluginContainer
	{
		CompositionContainer _pluginContainer;
		DirectoryCatalog _pluginCatalog;

		[ImportMany( AllowRecomposition = true )]
		public List<AbstractApplicationPersistence> _persistenceProviders { get; set; }

		public string PluginFolderPath
		{
			get { return _pluginCatalog.FullPath; }
		}

		/// <summary>
		///   Create a new persistence provider with application persistence plugins loaded from the specified path.
		/// </summary>
		/// <param name = "pluginFolderPath">The path where application persistence plugins are located.</param>
		public PersistenceProvider( string pluginFolderPath )
		{
			try
			{
				_pluginCatalog = CompositionHelper.CreateDirectory( pluginFolderPath );
				_pluginContainer = CompositionHelper.ComposeFromPath( this, _pluginCatalog );
			}
			catch ( CompositionException )
			{
				Dispose();
				throw;
			}
		}

		public void Refresh()
		{
			_pluginCatalog.Refresh();
		}

		protected IEnumerable<AbstractApplicationPersistence> GetPersistenceProviders1()
		{
			return _persistenceProviders;
		}

		public Version GetPluginVersion( Guid guid )
		{
			var plugin = GetAbstractApplicationPersistence( guid );
			return plugin != null ? plugin.AssemblyInfo.Version : null;
		}

		public string GetPluginPath( Guid guid )
		{
			return _pluginCatalog.LoadedFiles.FirstOrDefault( loadedFile => loadedFile.IndexOf( guid.ToString(), StringComparison.OrdinalIgnoreCase ) >= 0 );;
		}

		AbstractApplicationPersistence GetAbstractApplicationPersistence( Guid guid )
		{
			var plugin = GetPersistenceProviders1()
				.FirstOrDefault( persistance => persistance.AssemblyInfo.Guid == guid );
			return plugin;
		}

		public IInstallable GetInstallablePlugin( Guid guid )
		{
			return GetAbstractApplicationPersistence( guid ) as IInstallable;
		}


		protected override List<AbstractApplicationPersistence> GetPersistenceProviders()
		{
			return _persistenceProviders;
		}
	}
}