using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Whathecode.System.Extensions;
using Whathecode.System.Windows;


namespace ABC.Workspaces.Windows.Settings.ProcessBehavior
{
	public partial class ProcessBehaviors
	{
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
			var behaviors = (ProcessBehaviors)Serializer.Deserialize( s );

			behaviors.CommonIgnoreWindows = behaviors.CommonIgnoreWindows ?? new WindowList { Window = new Window[0] };
			behaviors.Process = behaviors.Process ?? new ProcessBehaviorsProcess[0];
			behaviors.Process.ForEach( process => process.Initialize() );

			return behaviors;
		}

		public void AddOrOverwriteProcess( ProcessBehaviorsProcess process )
		{
			var exisitingProcess = Process.FirstOrDefault( existingWindow => existingWindow.Equals( process ) );
			if ( exisitingProcess != null )
			{
				Process.Swap( exisitingProcess, process );
				return;
			}
			Process = Process.Concat( new[] { process } ).ToArray();
		}
	}

	public partial class ProcessBehaviorsProcess
	{
		public ProcessBehaviorsProcess()
		{
			Initialize();
		}

		public void Initialize()
		{
			IgnoreWindows = IgnoreWindows ?? new ProcessBehaviorsProcessIgnoreWindows { Window = new Window[0] };
			HideBehavior = HideBehavior ?? new ProcessBehaviorsProcessHideBehavior { Items = new object[0] };
		}

		bool _shouldHandleProcess = true;

		[XmlIgnoreAttribute]
		public Version TargetProcessVersionHelper
		{
			get
			{
				var version = new Version( "0.0" );
				try
				{
					version = new Version( targetProcessVersionField );
				}
				catch ( ArgumentException )
				{
					IsGeneral = true;
					// Ignore when value not provided, use 0.0 version.
				}
				catch ( FormatException )
				{
					IsGeneral = true;
					// Ignore bad format, use 0.0 version.
				}
				return version;
			}
		}

		[XmlIgnoreAttribute]
		public bool IsGeneral { get; private set; }

		[XmlIgnoreAttribute]
		public Version VersionHelper
		{
			get { return versionField == null ? new Version( "0.0.0.0" ) : new Version( versionField ); }
		}

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
				String.Equals( Version, other.Version ) &&
				Equals( TargetProcessVersionHelper, other.TargetProcessVersionHelper );
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = ( TargetProcessName != null ? TargetProcessName.GetHashCode() : 0 );
				hashCode = ( hashCode * 397 ) ^ ( TargetProcessCompanyName != null ? TargetProcessCompanyName.GetHashCode() : 0 );
				hashCode = ( hashCode * 397 ) ^ ( TargetProcessVersion != null ? TargetProcessVersion.GetHashCode() : 0 );
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

	public partial class WindowList
	{
		public bool AddIfAbsent( Window window )
		{
			if ( Window.Contains( window ) )
			{
				return false;
			}
			Window = Window.Concat( new[] { window } ).ToArray();
			return true;
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