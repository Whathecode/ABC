using ABC.Common;


namespace ABC.Managers.Windows
{
	/// <summary>
	///   Allows to execute window operations on windows managed by a virtual desktop manager.
	///   TODO: Could this be encapsulated from the VDM? The event passes <see cref="VirtualDesktop" /> instances.
	/// </summary>
	public interface IWindowOperations
	{
		/// <summary>
		///   Event which is triggered when in any virtual desktop unresponsive windows are detected.
		/// </summary>
		event UnresponsiveWindowsHandler UnresponsiveWindowDetected;

		/// <summary>
		///   Cut a given window and store it in a clipboard.
		/// </summary>
		void CutWindow( Window window );

		/// <summary>
		///   Paste all windows of the clipboard so they become visible again.
		/// </summary>
		void PasteWindows();
	}
}
