using System;
using System.Collections.Generic;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using ABC.Windows.Desktop.Server;


namespace ABC.Windows.Desktop.Monitor.Model
{
	class MonitorVdmClient
	{
		readonly IpcClientChannel _channel;
		readonly IMonitorVdmService _monitorVdm;

		public List<List<WindowInfo>> VirtualDesktops { get; private set; }
		public List<WindowInfo> WindowClipboard { get; private set; }


		public MonitorVdmClient()
		{
			// Set up communication with the virtual desktop monitoring server.
			_channel = new IpcClientChannel();
			ChannelServices.RegisterChannel( _channel, false );
			_monitorVdm = (IMonitorVdmService)Activator.GetObject( typeof( IMonitorVdmService ), "ipc://VirtualDesktopManagerAPI/MonitorVdmService" );
		}


		public bool UpdateData()
		{
			try
			{
				VirtualDesktops = _monitorVdm.GetWindowAssociations();
				WindowClipboard = _monitorVdm.GetWindowClipboard();
				return true;
			}
			catch ( RemotingException )
			{
				return false;
			}
		}

		public void CutWindows( List<WindowInfo> windows )
		{
			_monitorVdm.CutWindows( windows );
		}
	}
}
