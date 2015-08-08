using System;
using System.Collections.Generic;
using System.Reflection;
using ABC.Plugins;


namespace ABC.Interruptions
{
	/// <summary>
	///   Represents a handler which can check for incoming interruptions which might interrupt an ongoing activity.
	/// </summary>
	public abstract class AbstractInterruptionTrigger
	{
		public event Action<AbstractInterruption> InterruptionReceived = delegate { };

		protected AbstractInterruptionTrigger( Assembly assembly )
		{
			AssemblyInfo = new AssemblyInfo( assembly );
			if ( Guid.Empty == AssemblyInfo.Guid || String.IsNullOrEmpty( AssemblyInfo.TargetProcessName ) )
			{
				throw new ArgumentException( "Plug-in GUID and target process name have to be provided in assembly info." );
			}
		}

		public AssemblyInfo AssemblyInfo { get; private set; }

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