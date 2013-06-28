using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ABC.Windows;
using ABC.Windows.Desktop;


// TODO: Generating ProcessBehaviors with the correct namespace causes problems.
// ReSharper disable CheckNamespace
namespace Generated.ProcessBehaviors
// ReSharper restore CheckNamespace
{
	public partial class ProcessBehaviorsProcess
	{
		bool _shouldHandleProcess = true;


		public static ProcessBehaviorsProcess CreateDontHandleProcess()
		{
			return new ProcessBehaviorsProcess { _shouldHandleProcess = false };
		}


		public override bool Equals( object obj )
		{
			var other = obj as ProcessBehaviorsProcess;
			if ( other == null )
			{
				return false;
			}
			return ReferenceEquals( this, obj ) || Equals( other );
		}

		public bool Equals( ProcessBehaviorsProcess other )
		{
			return
				string.Equals( nameField, other.nameField ) &&
				string.Equals( companyNameField, other.companyNameField ) &&
				string.Equals( versionField, other.versionField );
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = (nameField != null ? nameField.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (companyNameField != null ? companyNameField.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (versionField != null ? versionField.GetHashCode() : 0);
				return hashCode;
			}
		}

		public bool ShouldHandleProcess()
		{
			return _shouldHandleProcess;
		}
	}

	public partial class Window
	{
		public bool Equals( WindowInfo window )
		{
			return
				ClassName == window.GetClassName() &&
				(Visible == WindowVisible.Both || (window.IsVisible() && Visible == WindowVisible.True));
		}
	}


	#region Cut behaviors

	public static class CutHelper
	{
		public static IEnumerable<WindowSnapshot> WindowsToSearchIn(DesktopManager desktopManager, ConsiderWindows selectedDesktops)
		{
			switch ( selectedDesktops )
			{
				case ConsiderWindows.AllWindows:
					return WindowManager.GetWindows().Select( w => new WindowSnapshot( w ) );
				case ConsiderWindows.AllDesktopWindows:
					return desktopManager.Desktops.SelectMany( d => d.WindowSnapshots );
				case ConsiderWindows.CurrentDesktopWindowsOnly:
					return desktopManager.CurrentDesktop.WindowSnapshots;
				default:
					throw new NotSupportedException();
			}
		}
	}

	public interface ICutBehavior
	{
		IEnumerable<WindowInfo> ToCut( WindowInfo windowInfo, DesktopManager desktopManager );
	}

	/// <summary>
	///   Default window selections, allowing to only cut the selected window, or all windows of the selected process.
	/// </summary>
	public partial class ProcessBehaviorsProcessHideBehaviorDefault : ICutBehavior
	{
		public IEnumerable<WindowInfo> ToCut( WindowInfo windowInfo, DesktopManager desktopManager )
		{
			var windows = new List<WindowInfo>();

			switch ( Hide )
			{
				case ProcessBehaviorsProcessHideBehaviorDefaultHide.SelectedWindow:
					{
						windows.Add( windowInfo );
						break;
					}
				case ProcessBehaviorsProcessHideBehaviorDefaultHide.AllProcessWindows:
					{
						var searchWindows = CutHelper.WindowsToSearchIn( desktopManager, ConsiderWindows );

						var processWindows = searchWindows.Where( dw =>
						{
							Process cutProcess = windowInfo.GetProcess();
							Process otherProcess = dw.Info.GetProcess();
							return
								cutProcess != null && otherProcess != null &&
							cutProcess.Id == otherProcess.Id;
						} );
						windows.AddRange( processWindows.Select( pw => pw.Info ) );
						break;
					}
			}

			return windows;
		}
	}

	/// <summary>
	///   Cut defined windows.
	/// </summary>
	public partial class ProcessBehaviorsProcessHideBehaviorInclude : ICutBehavior
	{
		public IEnumerable<WindowInfo> ToCut( WindowInfo windowInfo, DesktopManager desktopManager )
		{
			var searchWindows = CutHelper.WindowsToSearchIn( desktopManager, ConsiderWindows );
			return searchWindows.Where( s => Window.Any( w => w.Equals( s.Info ) ) ).Select( w => w.Info );
		}
	}

	#endregion // Cut behaviors
}
