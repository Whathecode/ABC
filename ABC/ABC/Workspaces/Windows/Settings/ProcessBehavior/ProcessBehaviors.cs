using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Whathecode.System.Windows;


namespace ABC.Workspaces.Windows.Settings.ProcessBehavior
{
	public partial class ProcessBehaviors
	{
		public ProcessBehaviors()
		{
			CommonIgnoreWindows = new WindowList { Window = new Window[0] };
		}

		static XmlSerializer _serializer;

		static XmlSerializer Serializer
		{
			get
			{
				if ( ( _serializer == null ) )
				{
					_serializer = new XmlSerializerFactory().CreateSerializer( typeof( ProcessBehaviors ) );
				}
				return _serializer;
			}
		}

		public static ProcessBehaviors Deserialize( Stream s )
		{
			return ( (ProcessBehaviors)( Serializer.Deserialize( s ) ) );
		}
	}

	public partial class ProcessBehaviorsProcess
	{
		public ProcessBehaviorsProcess()
		{
			IgnoreWindows = new ProcessBehaviorsProcessIgnoreWindows { Window = new Window[0] };
			HideBehavior = new ProcessBehaviorsProcessHideBehavior { Items = new object[0] };
		}

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
				String.Equals( TargetProcessName, other.TargetProcessName ) &&
				String.Equals( TargetProcessCompanyName, other.TargetProcessCompanyName ) &&
				String.Equals( TargerProcessVersion, other.TargerProcessVersion );
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = ( TargetProcessName != null ? TargetProcessName.GetHashCode() : 0 );
				hashCode = ( hashCode * 397 ) ^ ( TargetProcessCompanyName != null ? TargetProcessCompanyName.GetHashCode() : 0 );
				hashCode = ( hashCode * 397 ) ^ ( TargerProcessVersion != null ? TargerProcessVersion.GetHashCode() : 0 );
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
				( Visible == WindowVisible.Both || ( window.IsVisible() && Visible == WindowVisible.True ) ) &&
				( Title == null || window.GetTitle() == Title );
		}
	}


	#region Cut behaviors

	public static class CutHelper
	{
		public static IEnumerable<WindowInfo> WindowsToSearchIn( VirtualDesktopManager desktopManager, ConsiderWindows selectedDesktops )
		{
			switch ( selectedDesktops )
			{
				case ConsiderWindows.AllWindows:
					return WindowManager.GetWindows();
				case ConsiderWindows.AllDesktopWindows:
					return desktopManager.Workspaces.SelectMany( d => d.WindowSnapshots.Select( w => w.Info ) );
				case ConsiderWindows.CurrentDesktopWindowsOnly:
					return desktopManager.CurrentWorkspace.WindowSnapshots.Select( w => w.Info );
				default:
					throw new NotSupportedException();
			}
		}
	}

	public interface ICutBehavior
	{
		IEnumerable<WindowInfo> ToCut( WindowInfo windowInfo, VirtualDesktopManager desktopManager );
	}

	/// <summary>
	///   Default window selections, allowing to only cut the selected window, or all windows of the selected process.
	/// </summary>
	public partial class ProcessBehaviorsProcessHideBehaviorDefault : ICutBehavior
	{
		public IEnumerable<WindowInfo> ToCut( WindowInfo windowInfo, VirtualDesktopManager desktopManager )
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

					var processWindows = searchWindows.Where( w =>
					{
						Process cutProcess = windowInfo.GetProcess();
						Process otherProcess = w.GetProcess();
						return
							cutProcess != null && otherProcess != null &&
							cutProcess.Id == otherProcess.Id;
					} );
					windows.AddRange( processWindows );
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
		public IEnumerable<WindowInfo> ToCut( WindowInfo windowInfo, VirtualDesktopManager desktopManager )
		{
			var searchWindows = CutHelper.WindowsToSearchIn( desktopManager, ConsiderWindows );
			return searchWindows.Where( s => Window.Any( w => w.Equals( s ) ) );
		}
	}

	#endregion // Cut behaviors
}