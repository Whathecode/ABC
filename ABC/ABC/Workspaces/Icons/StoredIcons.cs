using System.Collections.Generic;
using System.Runtime.Serialization;
using ABC.PInvoke;


namespace ABC.Workspaces.Icons
{
	/// <summary>
	///   Allows storing the state of a <see cref = "DesktopIcons" />.
	/// </summary>
	/// <author>Steven Jeuris</author>
	[DataContract]
	public class StoredIcons
	{
		[DataMember]
		internal readonly string Folder;

		[DataMember]
		internal readonly List<DesktopIcon> Icons;


		public StoredIcons( DesktopIcons icons )
		{
			Folder = icons.Folder;
			Icons = icons.Icons;
		}
	}
}
