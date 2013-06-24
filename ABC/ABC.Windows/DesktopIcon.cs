
using System.Drawing;
namespace ABC.Windows
{
    public class DesktopIcon
    {
        public Point Location { get; set; }
        public int DesktopIndex { get; set; }

        public DesktopIcon() { }
        public DesktopIcon(int index, Point location)
        {
            DesktopIndex = index;
            Location = location;
        }
    }
}
