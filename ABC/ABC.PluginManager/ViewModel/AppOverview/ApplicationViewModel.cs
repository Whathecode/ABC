using System;
using System.Collections.Generic;
using System.Linq;
using PluginManager.Model;
using PluginManager.ViewModel.AppOverview.Binding;
using Whathecode.System.ComponentModel.NotifyPropertyFactory.Attributes;
using Whathecode.System.Windows.Aspects.ViewModel;


namespace PluginManager.ViewModel.AppOverview
{
	[ViewModel( typeof( ApplicationProperties ), typeof( ApplicationCommands ) )]
	public class ApplicationViewModel
	{
		public Guid Guid { get; private set; }

		[NotifyProperty( ApplicationProperties.Name )]
		public string Name { get; private set; }

		[NotifyProperty( ApplicationProperties.Author )]
		public string Author { get; private set; }

		[NotifyProperty( ApplicationProperties.Icon )]
		public string Icon { get; private set; }

		[NotifyProperty( ApplicationProperties.AnyVdm )]
		public bool AnyVdm { get; private set; }

		[NotifyProperty( ApplicationProperties.AnyInterruption )]
		public bool AnyInterruption { get; private set; }

		[NotifyProperty( ApplicationProperties.AnyPersistence )]
		public bool AnyPersistence { get; private set; }

		public ApplicationViewModel( PluginManifestApplication application, List<PluginManifestPlugin> applicationPlugins )
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