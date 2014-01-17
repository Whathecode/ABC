using System.Collections.Generic;
using System.Runtime.Serialization;
using Whathecode.System.Windows.Interop;


namespace ABC.Applications.Persistence
{
	/// <summary>
	///   Holds the state of a persisted application.
	/// </summary>
	[DataContract]
	public class PersistedApplication
	{
		[DataMember]
		public List<WindowInfo> Windows { get; private set; }

		[DataMember]
		public object Data { get; private set; }


		public PersistedApplication( List<WindowInfo> windows, object data )
		{
			Windows = windows;
			Data = data;
		}
	}
}
