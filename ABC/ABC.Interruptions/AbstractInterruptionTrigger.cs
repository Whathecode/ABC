﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using ABC.Applications;


namespace ABC.Interruptions
{
	/// <summary>
	///   Represents a handler which can check for incoming interruptions which might interrupt an ongoing activity.
	/// </summary>
	public abstract class AbstractInterruptionTrigger
	{
		[Import]
		protected ServiceProvider ServiceProvider { get; private set; }

		public event Action<AbstractInterruption> InterruptionReceived = delegate { };

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