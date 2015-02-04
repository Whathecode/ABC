using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;


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
		internal readonly List<Point> Icons;


		public StoredIcons( DesktopIcons icons )
		{
			Folder = icons.Folder;
			Icons = icons.Icons.ToList();
		}
	}
}
