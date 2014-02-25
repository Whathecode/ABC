﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using ABC.Common;


namespace ABC.Applications.Persistence
{
	/// <summary>
	///   Holds the state of a persisted application.
	/// </summary>
	[DataContract]
	public class PersistedApplication
	{
		internal Process Process;

		[DataMember]
		internal string ApplicationPath;

		[DataMember]
		internal string Persistor;

		[DataMember]
		public List<IWindow> Windows { get; private set; }

		[DataMember]
		public object Data { get; private set; }


		public PersistedApplication( Process process, List<IWindow> windows, object data )
		{
			Process = process;
			Windows = windows;
			Data = data;
		}
	}
}
