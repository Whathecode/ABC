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

		AbstractApplicationPersistence GetPluginByGuid( Guid guid )
		{
			return GetPersistenceProviders()
				.FirstOrDefault( persistance => persistance.AssemblyInfo.Guid == guid );
		}

		public Version GetPluginVersion( Guid guid )
		{
			var plugin = GetPluginByGuid( guid );
			return plugin != null ? plugin.AssemblyInfo.Version : null;
		}

		public bool InstallPugin( Guid guid )
		{
			var installablePlugin = GetPluginByGuid( guid ) as IInstallable;
			return installablePlugin == null || installablePlugin.Install();
		}

		public bool UninstallPugin( Guid guid )
		{
			var installablePlugin = GetPersistenceProviders()
				.FirstOrDefault( persistance => persistance.AssemblyInfo.Guid == guid ) as IInstallable;
			return installablePlugin == null || installablePlugin.Unistall();
		}


		protected override List<AbstractApplicationPersistence> GetPersistenceProviders()
		{
			return _persistenceProviders;
		}
	}
}