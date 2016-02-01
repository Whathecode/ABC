using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Threading;
using ABC.Plugins;


namespace ABC.Interruptions
{
	/// <summary>
	///   Aggregates interruptions raised by loaded interruption trigger plugins from a specified folder.
	/// </summary>
	[Guid( "7FE89184-0B45-4075-8D04-8234CBF3DAE1" )]
	public class FolderInterruptionTrigger : AbstractInterruptionTrigger
	{
		readonly FolderPluginComposition _pluginContainer;

		[ImportMany]
		readonly List<AbstractInterruptionTrigger> _interruptionTriggers = new List<AbstractInterruptionTrigger>();


		public FolderInterruptionTrigger( string pluginFolderPath )
		{
			_pluginContainer = new FolderPluginComposition( this, pluginFolderPath );

			// Initialize loaded interruption handlers.
			foreach ( var handler in _interruptionTriggers )
			{
				handler.InterruptionReceived += TriggerInterruption;
			}
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
	}
}