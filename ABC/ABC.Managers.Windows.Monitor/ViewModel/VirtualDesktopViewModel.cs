using System.Collections.Generic;
using System.Linq;
using Whathecode.System.Extensions;


namespace ABC.Managers.Windows.Monitor.ViewModel
{
	class VirtualDesktopViewModel
	{
		public string Name { get; private set; }
		public List<WindowViewModel> Windows { get; private set; }
		public List<WindowViewModel> SelectedWindows { get; private set; }


		public VirtualDesktopViewModel( string name, List<WindowViewModel> windows )
		{
			Name = name;
			Windows = windows;
			SelectedWindows = new List<WindowViewModel>();
		}


		public void ShowSelectedWindows()
		{
			SelectedWindows.Cast<WindowViewModel>().ForEach( w => w.Window.Show() );
		}

		public void HideSelectedWindows()
		{
			SelectedWindows.Cast<WindowViewModel>().ForEach( w => w.Window.Hide() );
		}
	}
}
