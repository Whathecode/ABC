using System.Runtime.Serialization;


namespace ABC.Windows.Desktop
{
	/// <summary>
	/// An internal representation of a WindowInfo object.
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
	public class WindowSnapshot
	{
		[DataMember]
		internal readonly WindowInfo Info;

		[DataMember]
		internal bool Visible;

		public WindowSnapshot( WindowInfo info )
		{
			Info = info;

			// TODO: This constructor only makes sense when called on windows from a currently visible desktop. Verify this, and attempt to enforce this.
			Update();
		}


		internal void Update()
		{
			Visible = Info.IsVisible();
		}

		public override bool Equals( object obj )
		{
			var other = obj as WindowSnapshot;
			if ( other == null )
			{
				return false;
			}

			return Equals( other );
		}

		protected bool Equals( WindowSnapshot other )
		{
			if ( ReferenceEquals( null, other ) )
			{
				return false;
			}

			return ReferenceEquals( this, other ) || Info.Equals( other.Info );
		}

		public override int GetHashCode()
		{
			return Info.GetHashCode();
		}
	}
}