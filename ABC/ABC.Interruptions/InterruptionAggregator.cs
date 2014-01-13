using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
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
			_pluginContainer = CompositionHelper.Compose( this, pluginFolderPath );

			// Remove invalid interruption handlers.
			var toRemove = new List<AbstractInterruptionTrigger>();
			foreach ( var trigger in _interruptionTriggers )
			{
				bool haveDataContracts = trigger.GetInterruptionTypes().All( i => i.GetCustomAttributes( false ).Any( a => a is DataContractAttribute ) );
				if ( !haveDataContracts )
				{
					string pluginName = trigger.GetType().ToString();
					Debug.WriteLine( "The interruptions triggered by plugin \"{0}\" should be serializable and have a DataContract attribute applied to them.", new object[] { pluginName } );
					toRemove.Add( trigger );
				}
			}
			toRemove.ForEach( t => _interruptionTriggers.Remove( t ) );

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
				try
				{
					trigger.Update( now );
				}
				catch ( Exception ex )
				{
					// Prevent main application from crashing when plugins throw exceptions.
					string pluginName = trigger.GetType().ToString();
					Debug.WriteLine( "The interruption trigger plugin \"{0}\" threw an exception:\n {1}", pluginName, ex );
				}
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
			return _interruptionTriggers.SelectMany( h => h.GetInterruptionTypes() ).ToList();
		}
	}
}
