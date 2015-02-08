using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using ABC.Applications.Persistence;
using ABC.Workspaces.Windows.Server;
using ABC.Workspaces.Windows.Settings;
using Whathecode.System.Extensions;
using Whathecode.System.Security.Principal;
using Whathecode.System.Windows;


namespace ABC.Workspaces.Windows
{
	public delegate void UnresponsiveWindowsHandler( List<WindowSnapshot> unresponsiveWindows, VirtualDesktop desktop );


	/// <summary>
	///   An <see cref = "AbstractWorkspaceManager{TWorkspace, TSession}" />
	///   which allows creating and switching between different <see cref = "VirtualDesktop" />'s.
	/// </summary>
	public class VirtualDesktopManager : AbstractWorkspaceManager<VirtualDesktop, StoredSession>, IWindowOperations
	{
		/// <summary>
		///   Event which is triggered when in any virtual desktop unresponsive windows are detected.
		/// </summary>
		public event UnresponsiveWindowsHandler UnresponsiveWindowDetected = delegate { };

		internal readonly Stack<WindowSnapshot> WindowClipboard = new Stack<WindowSnapshot>();

		readonly Func<Window, bool> _windowFilter;
		readonly Func<Window, VirtualDesktopManager, List<Window>> _hideBehavior;
		readonly List<WindowInfo> _invalidWindows = new List<WindowInfo>();
		readonly AbstractPersistenceProvider _persistenceProvider;

		// ReSharper disable NotAccessedField.Local
		readonly MonitorVdmPipeServer _monitorServer;
		// ReSharper restore NotAccessedField.Local


		/// <summary>
		///   Initializes a new desktop manager and creates one startup desktop containing all currently open windows.
		///   This desktop is accessible through the <see cref="AbstractWorkspaceManager{TWorkspace,TSession}.CurrentWorkspace" /> property.
		/// </summary>
		/// <param name = "settings">
		///   Contains settings for how the desktop manager should behave. E.g. which windows to ignore.
		/// </param>
		public VirtualDesktopManager( ISettings settings )
			: this( settings, new CollectionPersistenceProvider() ) {}

		/// <summary>
		///   Initializes a new desktop manager and creates one startup desktop containing all currently open windows.
		///   This desktop is accessible through the <see cref="AbstractWorkspaceManager{TWorkspace,TSession}.CurrentWorkspace" /> property.
		/// </summary>
		/// <param name = "settings">
		///   Contains settings for how the desktop manager should behave. E.g. which windows to ignore.
		/// </param>
		/// <param name = "persistenceProvider">Allows state of applications to be persisted and restored.</param>
		/// <exception cref = "BadImageFormatException">Thrown when other than x64 virtual desktop manager is run to operate on x64 platform.</exception>
		/// <exception cref = "NotSupportedException">Thrown when virtual desktop manager is started without necessary privileges.</exception>
		public VirtualDesktopManager( ISettings settings, AbstractPersistenceProvider persistenceProvider )
		{
			Contract.Requires( settings != null );

			_persistenceProvider = persistenceProvider;

			if ( !settings.IgnoreRequireElevatedPrivileges )
			{
				// ReSharper disable once AssignNullToNotNullAttribute, WindowsIdentity.GetCurrent() will never return null (http://stackoverflow.com/a/15998940/590790)
				var myPrincipal = new WindowsPrincipal( WindowsIdentity.GetCurrent() );
				if ( !myPrincipal.IsInRole( WindowsBuiltInRole.Administrator ) && WindowsIdentityHelper.IsUserInAdminGroup() )
				{
					throw new NotSupportedException(
						"The virtual desktop manager should be started with elevated privileges, otherwise it can't manage windows of processes with elevated privileges. " +
						"Alternatively, set 'IgnoreRequireElevantedPrivileges' to true in the settings passed to the VDM initialization, in which case these windows will be ignored entirely. " +
						"While debugging, we recommend running Visual Studio with elevated privileges as well." );
				}
			}
			if ( Environment.Is64BitOperatingSystem && !Environment.Is64BitProcess )
			{
				throw new BadImageFormatException( "A 64 bit version is needed for the Virtual Desktop Manager in order to run on a 64 bit platform." );
			}

			// Determine which windows shouldn't be managed by the desktop manager.
			_windowFilter = settings.CreateWindowFilter();
			_hideBehavior = settings.CreateHideBehavior();

			// Initialize visible startup desktop.
			var startupDesktop = new VirtualDesktop( _persistenceProvider );
			SetStartupWorkspace( startupDesktop );
			startupDesktop.UnresponsiveWindowDetected += OnUnresponsiveWindowDetected;
			startupDesktop.AddWindows( GetNewWindows() );

			_monitorServer = new MonitorVdmPipeServer( this );

			UpdateWindowAssociations();
		}

		void OnUnresponsiveWindowDetected( List<WindowSnapshot> unresponsiveWindows, VirtualDesktop desktop )
		{
			UnresponsiveWindowDetected( unresponsiveWindows, desktop );
		}

		void OnForceSuspendRequested( AbstractWorkspace<StoredSession> workspace )
		{
			var virtualDesktop = (VirtualDesktop)workspace;

			// TODO: Is there any way to always support this? The internal WindowSnapshot constructor assumes the windows are part of the currently visible desktop.
			if ( !virtualDesktop.IsVisible )
			{
				throw new InvalidOperationException( "Can only transfer windows when the desktop is currently visible." );
			}

			// Move windows from the forcefully suspended workspace to the startup workspace.
			List<WindowSnapshot> snapshots =  virtualDesktop.Windows.Select( w => new WindowSnapshot( virtualDesktop, w.WindowInfo ) ).ToList();
			virtualDesktop.RemoveWindows( snapshots );
			StartupWorkspace.AddWindows( snapshots );
		}

		protected override VirtualDesktop CreateEmptyWorkspaceInner()
		{
			var newDesktop = new VirtualDesktop( _persistenceProvider );
			HookDesktopEvents( newDesktop );
			return newDesktop;
		}

		protected override VirtualDesktop CreateWorkspaceFromSessionInner( StoredSession session )
		{
			// The startup desktop contains all windows open at startup.
			// Windows from previously stored sessions shouldn't be assigned to this startup desktop, so remove them.
			List<WindowSnapshot> otherWindows = StartupWorkspace.WindowSnapshots.Where( o => session.OpenWindows.Contains( o ) ).ToList();
			StartupWorkspace.RemoveWindows( otherWindows );

			var restored = new VirtualDesktop( session, _persistenceProvider );
			HookDesktopEvents( restored );
			session.OpenWindows.ForEach( w => w.ChangeDesktop( restored ) );
			return restored;
		}

		void HookDesktopEvents( VirtualDesktop desktop )
		{
			desktop.UnresponsiveWindowDetected += OnUnresponsiveWindowDetected;
			desktop.ForceSuspendRequested += OnForceSuspendRequested;
			desktop.Storing += d => UpdateWindowAssociations();
			desktop.SuspendingWorkspace += OnSuspendingWorkspace;
		}

		/// <summary>
		///   Update which windows are associated to the current virtual desktop.
		/// </summary>
		public void UpdateWindowAssociations()
		{
			ThrowExceptionIfDisposed();

			// The desktop needs to be visible in order to update window associations.
			if ( !CurrentWorkspace.IsVisible )
			{
				return;
			}

			// Update window associations for the currently open desktop.
			CurrentWorkspace.AddWindows( GetNewWindows() );

			CurrentWorkspace.RemoveWindows( CurrentWorkspace.WindowSnapshots.Where( w => !IsValidWindow( w ) ).ToList() );
			CurrentWorkspace.WindowSnapshots.ForEach( w => w.Update() );

			// Remove destroyed windows from places where they are cached.
			var destroyedWindows = _invalidWindows.Where( w => w.IsDestroyed() ).ToList();
			foreach ( var w in destroyedWindows )
			{
				lock ( _invalidWindows )
				{
					_invalidWindows.Remove( w );
				}
			}
		}

		void OnSuspendingWorkspace( AbstractWorkspace<StoredSession> workspace )
		{
			// Make sure the workspace is aware about all latest open windows.
			UpdateWindowAssociations();

			bool isSuspended = false;
			AbstractWorkspace<StoredSession>.WorkspaceEventHandler onSuspended = w => isSuspended = true;
			workspace.SuspendedWorkspace += onSuspended;

			// While awaiting full suspension in background, make sure list of remaining open windows is updated.
			var awaitSuspend = new BackgroundWorker();
			awaitSuspend.DoWork += ( sender, args ) =>
			{
				while ( !isSuspended )
				{
					UpdateWindowAssociations();
					Thread.Sleep( TimeSpan.FromSeconds( 1 ) );
				}
			};
			awaitSuspend.RunWorkerCompleted += ( sender, args ) =>
			{
				workspace.SuspendedWorkspace -= onSuspended;
			};
			awaitSuspend.RunWorkerAsync();
		}

		protected override void SwitchToWorkspaceInner( VirtualDesktop desktop )
		{
			UpdateWindowAssociations();

			// Hide windows for current desktop and show those from the new desktop.
			// TODO: Refactor so that one can not forget to call 'SetHideBehavior'.
			CurrentWorkspace.SetHideBehavior( wi => _hideBehavior( new Window( wi ), this ).Select( w => w.WindowInfo ).ToList() );
			CurrentWorkspace.Hide();
			desktop.Show();
		}

		protected override void MergeInner( VirtualDesktop from, VirtualDesktop to )
		{
			to.AddWindows( from.WindowSnapshots.ToList() );
		}

		protected override void CloseInner()
		{
			UpdateWindowAssociations(); // Make sure new visible windows are saved as well.

			// Show all desktops again.
			Workspaces.ForEach( w => w.Show() );

			// Show all cut windows again.
			var showWindows = WindowClipboard.Select( w => new RepositionWindowInfo( w.Info ) { Visible = w.Visible } ).ToList();
			try
			{
				WindowManager.RepositionWindows( showWindows, true, true );
			}
			catch ( UnresponsiveWindowsException e )
			{
				UnresponsiveWindowDetected(
					e.UnresponsiveWindows.Select( info => new WindowSnapshot( CurrentWorkspace, info ) ).ToList(), CurrentWorkspace );
			}

		}

		/// <summary>
		///   Check whether the WindowInfo object represents a valid Window.
		/// </summary>
		/// <returns>True if valid, false if unvalid.</returns>
		bool IsValidWindow( WindowSnapshot window )
		{
			return !window.Info.IsDestroyed() && _windowFilter( new Window( window.Info ) );
		}

		/// <summary>
		///   Gets all newly created windows, not handled by the desktop manager yet.
		/// </summary>
		/// <returns>A list with all new windows.</returns>
		List<WindowSnapshot> GetNewWindows()
		{
			List<WindowInfo> newWindows = WindowManager.GetWindows().Except(
				Workspaces.SelectMany( d => d.WindowSnapshots ).Concat( WindowClipboard )
					.Select( w => w.Info )
					.Concat( _invalidWindows ) ).ToList();

			var validWindows = new List<WindowSnapshot>();
			foreach ( var w in newWindows )
			{
				var snapshot = new WindowSnapshot( CurrentWorkspace, w );
				if ( IsValidWindow( snapshot ) )
				{
					validWindows.Add( snapshot );
				}
				else
				{
					lock ( _invalidWindows )
					{
						_invalidWindows.Add( w );
					}
				}
			}
			return validWindows;
		}

		/// <summary>
		///   Cut a given window from the currently open desktop and store it in a clipboard.
		///   TODO: What if a window from a different desktop is passed? Should this be supported?
		/// </summary>
		public void CutWindow( Window window )
		{
			ThrowExceptionIfDisposed();

			UpdateWindowAssociations(); // Newly added windows might need to be cut too, so update associations first.

			if ( !_windowFilter( window ) )
			{
				return;
			}

			var cutWindows = _hideBehavior( window, this ).Select( w => new WindowSnapshot( CurrentWorkspace, w.WindowInfo ) ).ToList();
			cutWindows.ForEach( w => WindowClipboard.Push( w ) );

			CurrentWorkspace.RemoveWindows( cutWindows );
		}

		/// <summary>
		///   Paste all windows on the clipboard on the currently open desktop.
		/// </summary>
		public void PasteWindows()
		{
			ThrowExceptionIfDisposed();

			UpdateWindowAssociations(); // There might be newly added windows of which the z-order isn't known yet, so update associations first.

			CurrentWorkspace.AddWindows( WindowClipboard.ToList() );
		}

		protected override void FreeManagedResources()
		{
			base.FreeManagedResources();

			_persistenceProvider.Dispose();
		}

		protected override void FreeUnmanagedResources()
		{
			// Nothing to do.
		}
	}
}
