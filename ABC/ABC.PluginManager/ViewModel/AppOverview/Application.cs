using System;
using System.Collections.Generic;
using System.Linq;
using PluginManager.Model;


namespace PluginManager.ViewModel.AppOverview
{
	public class Application
	{
		public Guid Guid { get; private set; }
		public string Name { get; private set; }
		public string Author { get; private set; }
		public string Icon { get; private set; }


		public bool AnyVdm { get; private set; }
		public bool AnyInterruption { get; private set; }
		public bool AnyPersistence { get; private set; }

		public Application( PluginManifestApplication application, List<PluginManifestPlugin> applicationPlugins )
		{
			Guid = new Guid( application.Guid );
			Name = application.Name;
			Author = application.Author;
			Icon = application.Icon;

			AnyInterruption = applicationPlugins.Any( plugin => plugin.Type == PluginType.Interruption );
			AnyVdm = applicationPlugins.Any( plugin => plugin.Type == PluginType.Vdm );
			AnyPersistence = applicationPlugins.Any( plugin => plugin.Type == PluginType.Persistence );
		}
	}
}