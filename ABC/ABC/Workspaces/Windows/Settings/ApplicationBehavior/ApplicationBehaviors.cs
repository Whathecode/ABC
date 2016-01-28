using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Whathecode.System.Extensions;
using Whathecode.System.Windows;


namespace ABC.Workspaces.Windows.Settings.ApplicationBehavior
{
	public partial class ApplicationBehaviors
	{
		/// <summary>
		///   Create an empty <see cref="ApplicationBehaviors" />, not containing any process configurations.
		/// </summary>
		public ApplicationBehaviors()
		{
			Initialize();
		}

		void Initialize()
		{
			// Initialize all containing collections as empty when null. (OnDeserializedAttribute and IDeserializationCallback don't work with XML.)
			CommonIgnoreWindows = CommonIgnoreWindows ?? new WindowList { Window = new Window[ 0 ] };
			Process = Process ?? new ApplicationBehaviorsProcess[ 0 ];
			Process.ForEach( p =>
			{
				p.IgnoreWindows = p.IgnoreWindows ?? new ApplicationBehaviorsProcessIgnoreWindows();
				p.HideBehavior = p.HideBehavior ?? new ApplicationBehaviorsProcessHideBehavior { Items = new object[ 0 ] };
				p.IgnoreWindows.Window = p.IgnoreWindows.Window ?? new Window[ 0 ];
			} );
		}

		public static ApplicationBehaviors Deserialize( Stream s )
		{
			var serializer = new XmlSerializer( typeof( ApplicationBehaviors ) );
			var behaviors = (ApplicationBehaviors)serializer.Deserialize( s );
			behaviors.Initialize();

			return behaviors;
		}
	}


	public partial class ApplicationBehaviorsProcess
	{
		bool _shouldHandleProcess = true;
		public bool ShouldHandleProcess { get { return _shouldHandleProcess; } }


		public static ApplicationBehaviorsProcess CreateHandleProcess()
		{
			return CreateEmpty();
		}

		public static ApplicationBehaviorsProcess CreateDontHandleProcess()
		{
			var process = CreateEmpty();
			process._shouldHandleProcess = false;
			return process;
		}

		static ApplicationBehaviorsProcess CreateEmpty()
		{
			var process = new ApplicationBehaviorsProcess
			{
				IgnoreWindows = new ApplicationBehaviorsProcessIgnoreWindows { Window = new Window[ 0 ] },
				HideBehavior = new ApplicationBehaviorsProcessHideBehavior { Items = new object[ 0 ] }
			};
			return process;
		}

		public override bool Equals( object obj )
		{
			var other = obj as ApplicationBehaviorsProcess;
			if ( other == null )
			{
				return false;
			}
			return ReferenceEquals( this, obj ) || Equals( other );
		}

		public bool Equals( ApplicationBehaviorsProcess other )
		{
			return
				String.Equals( Name, other.Name ) &&
				String.Equals( CompanyName, other.CompanyName ) &&
				String.Equals( Version, other.Version );
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = Name?.GetHashCode() ?? 0;
				hashCode = ( hashCode * 397 ) ^ ( CompanyName?.GetHashCode() ?? 0 );
				hashCode = ( hashCode * 397 ) ^ ( Version?.GetHashCode() ?? 0 );
				return hashCode;
			}
		}
	}


	public partial class Window
	{
		public override bool Equals( object obj )
		{
			var other = obj as Window;
			if ( other == null )
			{
				return false;
			}
			return ReferenceEquals( this, obj ) || Equals( other );
		}

		public override int GetHashCode()
		{
			unchecked
			{
				// ReSharper disable NonReadonlyMemberInGetHashCode
				int hashCode = classNameField?.GetHashCode() ?? 0;
				hashCode = ( hashCode * 397 ) ^ ( titleField?.GetHashCode() ?? 0 );
				hashCode = ( hashCode * 397 ) ^ visibleField.GetHashCode();
				// ReSharper restore NonReadonlyMemberInGetHashCode
				return hashCode;
			}
		}

		public bool Equals( Window other )
		{
			return
				String.Equals( classNameField, other.classNameField ) &&
				String.Equals( titleField, other.titleField ) &&
				visibleField == other.visibleField;
		}

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
	public partial class ApplicationBehaviorsProcessHideBehaviorDefault : ICutBehavior
	{
		public IEnumerable<WindowInfo> ToCut( WindowInfo windowInfo, VirtualDesktopManager desktopManager )
		{
			var windows = new List<WindowInfo>();

			switch ( Hide )
			{
				case ApplicationBehaviorsProcessHideBehaviorDefaultHide.SelectedWindow:
				{
					windows.Add( windowInfo );
					break;
				}
				case ApplicationBehaviorsProcessHideBehaviorDefaultHide.AllProcessWindows:
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
	public partial class ApplicationBehaviorsProcessHideBehaviorInclude : ICutBehavior
	{
		public IEnumerable<WindowInfo> ToCut( WindowInfo windowInfo, VirtualDesktopManager desktopManager )
		{
			var searchWindows = CutHelper.WindowsToSearchIn( desktopManager, ConsiderWindows );
			return searchWindows.Where( s => Window.Any( w => w.Equals( s ) ) );
		}
	}

	#endregion // Cut behaviors
}