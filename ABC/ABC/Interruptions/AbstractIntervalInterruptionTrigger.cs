using System;
using System.Reflection;


namespace ABC.Interruptions
{
	/// <summary>
	///  An interruption handler which only updates its state at specified intervals.
	/// </summary>
	public abstract class AbstractIntervalInterruptionTrigger : AbstractInterruptionTrigger
	{
		readonly TimeSpan _interval;
		DateTime? _lastUpdate;


		protected AbstractIntervalInterruptionTrigger( TimeSpan interval, Assembly assembly )
			: base( assembly )
		{
			_interval = interval;
		}


		public override void Update( DateTime now )
		{
			if ( _lastUpdate != null && ( now - _lastUpdate.Value ) < _interval )
			{
				return;
			}

			_lastUpdate = now;
			IntervalUpdate( now );
		}

		protected abstract void IntervalUpdate( DateTime now );
	}
}