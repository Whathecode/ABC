using System;
using System.Runtime.Serialization;


namespace ABC.Interruptions
{
	/// <summary>
	///   An interruption which interrupts an ongoing activity.
	/// </summary>
	[DataContract]
	public abstract class AbstractInterruption
	{
		[DataMember]
		public string Name { get; private set; }

		/// <summary>
		///   The time when the interruption was triggered.
		/// </summary>
		[DataMember]
		public DateTime TriggeredAt { get; private set; }

		/// <summary>
		///   Indicates whether or not the interruption has been attended to.
		/// </summary>
		[DataMember]
		public bool AttendedTo { get; private set; }


		protected AbstractInterruption( string name )
		{
			Name = name;
			TriggeredAt = DateTime.Now;
		}
	}
}
