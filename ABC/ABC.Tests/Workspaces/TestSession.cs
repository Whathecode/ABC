using System.Runtime.Serialization;


namespace ABC.Workspaces
{
	[DataContract]
	class TestSession
	{
		[DataMember]
		public string SomeSerializableProperty { get; private set; }
	}
}
