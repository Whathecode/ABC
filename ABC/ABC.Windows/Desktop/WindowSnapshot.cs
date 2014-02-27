using System;
using System.Runtime.Serialization;
using Whathecode.System.Windows.Interop;


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
	class WindowSnapshot
	{
		internal VirtualDesktop Desktop { get; private set; }

		[DataMember]
		internal readonly WindowInfo Info;

		[DataMember]
		internal bool Visible { get; private set; }


		/// <summary>
		///   Creates a new snapshot for a window currently visible on the open desktop.
		///   This constructor should only be called on windows assigned to a currently visible desktop.
		/// </summary>
		/// <param name="currentDesktop">The desktop the window is assigned to.</param>
		/// <param name="info">The open window</param>
		internal WindowSnapshot( VirtualDesktop currentDesktop, WindowInfo info )
		{
			if ( !currentDesktop.IsVisible )
			{
				throw new InvalidOperationException( "Window snapshots can only be created when the desktop the window belongs to is currently visible." );
			}

			Desktop = currentDesktop;
			Info = info;

			Update();
		}


		/// <summary>
		///   Updates the snapshot. Visibility is only updated in case the desktop the window belongs to is currently visible.
		/// </summary>
		internal void Update()
		{
			if ( Desktop.IsVisible )
			{
				Visible = Info.IsVisible();
			}
		}

		public void ChangeDesktop( VirtualDesktop newDesktop )
		{
			Desktop = newDesktop;
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