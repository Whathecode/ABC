using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using ABC.Applications.Persistence;
using ABC.Plugins;


namespace ABC.Plugins.Manager
{
	/// <summary>
	///   Provides persistence providers for supported applications which are loaded and managed by the plugin manager.
	/// </summary>
	public class PluginPersistenceProvider : AbstractPersistenceProvider, IInstallablePluginContainer
	{
		readonly FolderPluginComposition _pluginContainer;

		[ImportMany( AllowRecomposition = true )]
		readonly List<AbstractApplicationPersistence> _persistenceProviders = new List<AbstractApplicationPersistence>();

		public string PluginFolderPath { get { return _pluginContainer.Folder; } }


		/// <summary>
		///   Create a new persistence provider with application persistence plugins loaded from the specified path.
		/// </summary>
		/// <param name = "pluginFolderPath">The path where application persistence plugins are located.</param>
		public PluginPersistenceProvider( string pluginFolderPath )
		{
			try
			{
				_pluginContainer = new FolderPluginComposition( this, pluginFolderPath );
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

		public void Refresh()
		{
			_pluginContainer.Refresh();
		}

		public IInstallable GetInstallablePlugin( Guid guid )
		{
			throw new NotImplementedException();
		}

		public Version GetPluginVersion( Guid guid )
		{
			throw new NotImplementedException();
		}

		public string GetPluginPath( Guid guid )
		{
			return _pluginContainer.GetLoadedFiles().FirstOrDefault( loadedFile => loadedFile.IndexOf( guid.ToString(), StringComparison.OrdinalIgnoreCase ) >= 0 );;
		}

		protected override void FreeManagedResources()
		{
			base.FreeManagedResources();

			_pluginContainer?.Dispose();
		}
	}
}