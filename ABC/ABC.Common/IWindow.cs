﻿using System;
using System.Diagnostics;


namespace ABC.Common
{
	/// <summary>
	///   Interface which represents an application window.
	/// </summary>
	/// <author>Steven Jeuris</author>
	public interface IWindow
	{
		/// <summary>
		///   The underlying window handle.
		/// </summary>
		IntPtr Handle { get; }

		/// <summary>
		///   Retrieves the process that created the window, when available.
		/// </summary>
		/// <returns>The process when available, null otherwise.</returns>
		Process GetProcess();
	}
}