using System.Runtime.Serialization;

namespace ABC.Windows.Desktop
{
	/// <summary>
	///   Represents a window and its state in a <see cref="VirtualDesktop" />.
	/// </summary>
	/// <author>Steven Jeuris</author>
	/// <license>
	///   This file is part of VirtualDesktopManager.
	///   VirtualDesktopManager is free software: you can redistribute it and/or modify
	///   it under the terms of the GNU General Public License as published by
	///   the Free Software Foundation, either version 3 of the License, or
	///   (at your option) any later version.
	///
	///   VirtualDesktopManager is distributed in the hope that it will be useful,
	///   but WITHOUT ANY WARRANTY; without even the implied warranty of
	///   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	///   GNU General Public License for more details.
	///
	///   You should have received a copy of the GNU General Public License
	///   along with VirtualDesktopManager.  If not, see http://www.gnu.org/licenses/.
	/// </license>
	[DataContract]
	public class Window
	{
		[DataMember]
		internal readonly WindowInfo Info;

		[DataMember]
		internal bool Visible;


		public Window( WindowInfo info )
		{
			Info = info;
			Update();
		}


		public void Update()
		{
			Visible = Info.IsVisible();
		}
	}
}
