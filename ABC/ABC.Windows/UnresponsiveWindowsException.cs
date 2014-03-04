using System;
using System.Collections.Generic;
using ABC.Windows.Desktop;
using Whathecode.System.Extensions;


namespace ABC.Windows
{
	/// <summary>
	///   Exception which is thrown when one or more windows managed by the virtual desktop manager stop responding.
	///   By setting 'Ignore' of <see cref="UnresponsiveWindows" /> to true, these windows will be ignored by the desktop manager from then on, allowing to recover from the problem.
	///   To ignore all unresponsive windows, call <see cref="IgnoreAllWindows" />.
	/// </summary>
	public class UnresponsiveWindowsException : Exception
	{
		/// <summary>
		///   The desktop which the windows which are unresponsive belong to.
		/// </summary>
		public VirtualDesktop ParentDesktop { get; private set; }

		/// <summary>
		///   The unresponsive windows which are causing the virtual desktop manager to hang.
		/// </summary>
		public IReadOnlyCollection<WindowSnapshot> UnresponsiveWindows { get; private set; }


		public UnresponsiveWindowsException( VirtualDesktop desktop, List<WindowSnapshot> unresponsiveWindows )
		{
			ParentDesktop = desktop;
			UnresponsiveWindows = unresponsiveWindows;
		}


		public void IgnoreAllWindows()
		{
			UnresponsiveWindows.ForEach( w => w.Ignore = true );
		}
	}
}
