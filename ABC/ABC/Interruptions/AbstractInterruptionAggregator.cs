using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using ABC.Plugins;


namespace ABC.Interruptions
{
	public abstract class AbstractInterruptionAggregator
	{
		public event EventHandler<AbstractInterruption> InterruptionReceived;

		/// <summary>
		///   The <see cref = "AbstractInterruptionTrigger.InterruptionReceived" /> event returns interruption types which are serializable.
		///   In order to serialize them however, a <see cref = "DataContractSerializer" /> needs to be aware of the exact types.
		///   These are returned by this function.
		/// </summary>
		/// <returns>All the interruption types this interruption aggregator knows about.</returns>
		public List<Type> GetInterruptionTypes()
		{
			return GetInterruptionTriggers()
				.SelectMany( h => PluginHelper<AbstractInterruptionTrigger>.SafePluginInvoke( h, t => t.GetInterruptionTypes() ) )
				.ToList();
		}

		public void Update( DateTime now )
		{
			if ( !Monitor.TryEnter( this ) )
			{
				return;
			}

			GetInterruptionTriggers().ForEach( trigger =>
				PluginHelper<AbstractInterruptionTrigger>.SafePluginInvoke( trigger, t => t.Update( now ) ) );

			Monitor.Exit( this );
		}

		protected void HookInterruptionTriggers()
		{
			GetInterruptionTriggers().ForEach( trigger => trigger.InterruptionReceived += TriggerOnInterruptionReceived );
		}

		protected void UnhookInterruptionTriggers()
		{
			GetInterruptionTriggers().ForEach( trigger => trigger.InterruptionReceived -= TriggerOnInterruptionReceived );
		}

		void TriggerOnInterruptionReceived( object sender, AbstractInterruption abstractInterruption )
		{
			InterruptionReceived( this, abstractInterruption );
		}

		protected abstract List<AbstractInterruptionTrigger> GetInterruptionTriggers();
	}
}