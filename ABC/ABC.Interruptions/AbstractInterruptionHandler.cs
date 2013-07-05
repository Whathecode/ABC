using System;


namespace ABC.Interruptions
{
	/// <summary>
	///   Represents a handler which can check for incoming interruptions which might interrupt an ongoing activity.
	/// </summary>
	public abstract class AbstractInterruptionHandler
	{
		public event Action<string> InterruptionReceived;

		public abstract void Update( DateTime now );

		protected void TriggerInterruption( string name )
		{
			InterruptionReceived( name );
		}
	}
}