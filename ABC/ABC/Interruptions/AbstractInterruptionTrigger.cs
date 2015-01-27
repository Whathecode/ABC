using System;
using System.Collections.Generic;
using ABC.Common;


namespace ABC.Interruptions
{
	/// <summary>
	///   Represents a handler which can check for incoming interruptions which might interrupt an ongoing activity.
	/// </summary>
	public abstract class AbstractInterruptionTrigger
	{
		public event Action<AbstractInterruption> InterruptionReceived = delegate { };

		protected AbstractInterruptionTrigger() { }

		protected AbstractInterruptionTrigger( PluginInformation info )
		{
			Info = info;
		}
		
		internal PluginInformation Info { get; private set; }

		protected void TriggerInterruption( AbstractInterruption interruption )
		{
			InterruptionReceived( interruption );
		}

		public abstract void Update( DateTime now );

		/// <summary>
		///   Returns the types of interruptions this class triggers.
		/// </summary>
		public abstract List<Type> GetInterruptionTypes();
	}
}