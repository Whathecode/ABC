using System.Diagnostics;
using Whathecode.System.Windows.Interop;


namespace ABC.Windows.Desktop.Monitor.ViewModel
{
	class WindowViewModel
	{
		public WindowInfo Window { get; private set; }

		public string ProcessName { get; private set; }
		public string ClassName { get; private set; }
		public string Title { get; private set; }
		public bool IsVisible { get; private set; }


		public WindowViewModel( WindowInfo window )
		{
			Window = window;

			Process process = window.GetProcess();
			ProcessName = process != null ? process.ProcessName : "Not Found!";
			ClassName = window.GetClassName();
			Title = window.GetTitle();
			IsVisible = window.IsVisible();
		}
	}
}
