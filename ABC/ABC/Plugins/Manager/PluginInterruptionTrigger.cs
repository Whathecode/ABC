using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Threading;
using ABC.Interruptions;


namespace ABC.Plugins.Manager
{
	/// <summary>
	///   Aggregates interruptions raised by plugins which are loaded and managed by the plugin manager.
	/// </summary>
	[Guid( "97E13023-2F8C-460F-B715-717373AE4176" )]
	public class PluginInterruptionTrigger : AbstractInterruptionTrigger, IInstallablePluginContainer
	{
		readonly FolderPluginComposition _pluginContainer;

		[ImportMany( AllowRecomposition = true )]
		readonly List<AbstractInterruptionTrigger> _interruptionTriggers = new List<AbstractInterruptionTrigger>();

		public string PluginFolderPath { get { return _pluginContainer.Folder; } }


		public PluginInterruptionTrigger( string pluginFolderPath )
		{
			_pluginContainer = new FolderPluginComposition( this, pluginFolderPath );

			HookInterruptionTriggers();
		}


		void HookInterruptionTriggers()
		{
			_interruptionTriggers.ForEach( trigger => trigger.InterruptionReceived += TriggerInterruption );
		}

		void UnhookInterruptionTriggers()
		{
			_interruptionTriggers.ForEach( trigger => trigger.InterruptionReceived -= TriggerInterruption );
		}

		public override void Update( DateTime now )
		{
			if ( !Monitor.TryEnter( this ) )
			{
				return;
			}

			_interruptionTriggers.ForEach( trigger => PluginHelper.SafePluginInvoke( trigger, t => t.Update( now ) ) );

			Monitor.Exit( this );
		}

		/// <summary>
		///   The <see cref = "AbstractInterruptionTrigger.InterruptionReceived" /> event returns interruption types which are serializable.
		///   In order to serialize them however, a <see cref = "DataContractSerializer" /> needs to be aware of the exact types.
		///   These are returned by this function.
		/// </summary>
		/// <returns>All the interruption types this interruption aggregator knows about.</returns>
		public override List<Type> GetInterruptionTypes()
		{
			return _interruptionTriggers
				.SelectMany( h => PluginHelper.SafePluginInvoke( h, t => t.GetInterruptionTypes() ) )
				.ToList();
		}

		public void Refresh()
		{
			UnhookInterruptionTriggers();
			_pluginContainer.Refresh();
			HookInterruptionTriggers();
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
			return _pluginContainer.GetLoadedFiles().FirstOrDefault( loadedFile => loadedFile.IndexOf( guid.ToString(), StringComparison.OrdinalIgnoreCase ) >= 0 );
		}
	}
}