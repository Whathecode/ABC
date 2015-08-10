using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using ABC.Plugins;


namespace ABC.Interruptions
{
	/// <summary>
	///   Aggregates interruptions raised by externally loaded plug-ins.
	/// </summary>
	public class InterruptionAggregator : AbstractInterruptionAggregator, IInstallablePluginContainer
	{
		readonly DirectoryCatalog _pluginCatalog;

		[ImportMany( AllowRecomposition = true )]
		readonly List<AbstractInterruptionTrigger> _interruptionTriggers =
			new List<AbstractInterruptionTrigger>();


		public string PluginFolderPath
		{
			get { return _pluginCatalog.FullPath; }
		}

		public InterruptionAggregator( string pluginFolderPath )
		{
			_pluginCatalog = CompositionHelper.CreateDirectory( pluginFolderPath );
			CompositionHelper.ComposeFromPath( this, _pluginCatalog );

			HookInterruptionTriggers();
		}

		public void Refresh()
		{
			UnhookInterruptionTriggers();
			_pluginCatalog.Refresh();
			HookInterruptionTriggers();
		}

		AbstractInterruptionTrigger GetInterruptionTrigger( Guid guid )
		{
			return _interruptionTriggers
				.FirstOrDefault( interruption => interruption.AssemblyInfo.Guid == guid );
		}

		public Version GetPluginVersion( Guid guid )
		{
			var plugin = GetInterruptionTrigger( guid );
			return plugin != null ? plugin.AssemblyInfo.Version : null;
		}

		public IInstallable GetInstallablePlugin( Guid guid )
		{
			// ReSharper disable once SuspiciousTypeConversion.Global
			return GetInterruptionTrigger( guid ) as IInstallable;
		}

		public string GetPluginPath( Guid guid )
		{
			return _pluginCatalog.LoadedFiles.FirstOrDefault( loadedFile => loadedFile.IndexOf( guid.ToString(), StringComparison.OrdinalIgnoreCase ) >= 0 );
		}

		protected override List<AbstractInterruptionTrigger> GetInterruptionTriggers()
		{
			return _interruptionTriggers;
		}
	}
}