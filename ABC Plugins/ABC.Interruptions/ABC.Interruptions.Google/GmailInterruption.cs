using System;
using System.Collections.Generic;
using System.Runtime.Serialization;


namespace ABC.Interruptions.Google
{
	[DataContract]
	public class GmailInterruption : AbstractInterruption
	{
		[DataMember]
		readonly string _link;


		public GmailInterruption( string name, string content, string link, string issued, List<string> collaborators, List<string> files )
			: base( name )
		{
			Content = content;
			Collaborators = collaborators;
			_link = link;
			TriggeredAt = DateTime.Parse( issued );
			Files = files;
		}


		protected override void OnInterruptionOpened()
		{
			ServiceProvider.Browser.OpenWebsite( _link );
		}
	}
}
