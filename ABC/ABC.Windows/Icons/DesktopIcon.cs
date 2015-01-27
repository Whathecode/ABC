using System.Drawing;
using System.Runtime.Serialization;


namespace ABC.Windows.Icons
{
	[DataContract]
	public class DesktopIcon
	{
		[DataMember]
		public Point Location { get; private set; }

		[DataMember]
		public int DesktopIndex { get; private set; }


		public DesktopIcon( int index, Point location )
		{
			DesktopIndex = index;
			Location = location;
		}
	}
}