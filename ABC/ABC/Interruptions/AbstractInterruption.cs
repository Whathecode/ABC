using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Reflection;
using System.Runtime.Serialization;
using ABC.Applications.Services;


namespace ABC.Interruptions
{
	/// <summary>
	///   An interruption which interrupts an ongoing activity.
	/// </summary>
	[DataContract]
	public abstract class AbstractInterruption
	{
		[Import]
		protected ServiceProvider ServiceProvider { get; private set; }

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
			SetDefaults();

			Name = name;
			TriggeredAt = DateTime.Now;
		}


		[OnDeserializing]
		void OnDeserializing( StreamingContext context )
		{
			SetDefaults();
		}

		void SetDefaults()
		{
			var container = new CompositionContainer( new AssemblyCatalog( Assembly.GetEntryAssembly() ) );
			container.ComposeParts( this );
		}

		public void Open()
		{
			OnInterruptionOpened();

			AttendedTo = true;
		}

		protected abstract void OnInterruptionOpened();
	}
}
