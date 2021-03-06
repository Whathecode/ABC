﻿using System.Diagnostics;
using Whathecode.System.Windows;


namespace ABC.Workspaces.Windows.Monitor.ViewModel
{
	class WindowViewModel
	{
		public WindowInfo Window { get; private set; }

		public string ProcessName { get; private set; }
		public string CompanyName { get; private set; }
		public string ClassName { get; private set; }
		public string Title { get; private set; }
		public bool IsVisible { get; private set; }


		public WindowViewModel( WindowInfo window )
		{
			Window = window;

			Process process = window.GetProcess();
			ProcessName = process?.ProcessName ?? "Not Found!";
			CompanyName = process?.MainModule.FileVersionInfo.CompanyName ?? "";
			ClassName = window.GetClassName();
			Title = window.GetTitle();
			IsVisible = window.IsVisible();
		}
	}
}
