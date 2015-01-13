using System.Runtime.Serialization;


namespace ABC.Tests
{
	[DataContract]
	class TestSession
	{
		[DataMember]
		public string SomeSerializableProperty { get; private set; }
	}
}
