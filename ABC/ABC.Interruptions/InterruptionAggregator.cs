using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using ABC.Common;


namespace ABC.Interruptions
{
	/// <summary>
	///   Aggregates interruptions raised by externally loaded plugins.
	/// </summary>
	public class InterruptionAggregator : AbstractInterruptionTrigger
	{
		readonly CompositionContainer _pluginContainer;

		[ImportMany]
		readonly List<AbstractInterruptionTrigger> _interruptionTriggers = new List<AbstractInterruptionTrigger>();


		public InterruptionAggregator( string pluginFolderPath )
		{
			_pluginContainer = CompositionHelper.ComposeFromPath( this, pluginFolderPath );

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

			foreach ( var trigger in _interruptionTriggers )
			{
				PluginHelper<AbstractInterruptionTrigger>.SafePluginInvoke( trigger, t => t.Update( now ) );
			}

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
				.SelectMany( h => PluginHelper<AbstractInterruptionTrigger>.SafePluginInvoke( h, t => t.GetInterruptionTypes() ) )
				.ToList();
		}
	}
}
