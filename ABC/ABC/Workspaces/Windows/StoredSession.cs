﻿using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using ABC.Applications.Persistence;


namespace ABC.Workspaces.Windows
{
	/// <summary>
	///   Allows storing the state of a <see cref = "VirtualDesktop" />.
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
	public class StoredSession
	{
		[DataMember]
		internal bool IsStartupDesktop;

		/// <summary>
		///   Holds all the windows open on the virtual desktop.
		/// </summary>
		[DataMember]
		internal readonly ReadOnlyCollection<WindowSnapshot> OpenWindows;

		/// <summary>
		///   Holds the persisted application data.
		/// </summary>
		[DataMember]
		internal ReadOnlyCollection<PersistedApplication> PersistedApplications;


		internal StoredSession( VirtualDesktop desktop )
		{
			IsStartupDesktop = desktop.IsStartupDesktop;
			OpenWindows = desktop.WindowSnapshots;
			PersistedApplications = desktop.PersistedApplications;
		}
	}
}