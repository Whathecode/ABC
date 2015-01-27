using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;


namespace ABC.Managers.Windows.Server
{
	/// <summary>
	///   An inter process communication server to which clients can connect to monitor which windows are associated to the virtual desktops.
	/// </summary>
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
	class MonitorVdmPipeServer
	{
		readonly IpcServerChannel _channel = new IpcServerChannel( "VirtualDesktopManagerAPI" );


		public MonitorVdmPipeServer( VirtualDesktopManager desktopManager )
		{
			ChannelServices.RegisterChannel( _channel, false );
			MonitorVdmService monitorVdm = new MonitorVdmService( desktopManager );
			RemotingServices.Marshal( monitorVdm, "MonitorVdmService" );
		}
	}
}