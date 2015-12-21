using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;
using System.Security.Permissions;
using System.Threading;
using ABC.Applications.Persistence;
using ABC.Interruptions;
using ABC.Plugins;
using ABC.Workspaces.Windows.Settings;
using PluginManager.Model;


namespace PluginManager.PluginManagment
{
	public class PluginProvider : MarshalByRefObject, IDisposable
	{
		const string InstalledErrorMessage = "Plug-in was not installed correctly.";
		const string UninstalledErrorMessage = "Plug-in was not uninstalled correctly.";
		const string WatcherErrorMessage = "Plug-in directory watcher encountered a problem.";
		const string CannotExtractFromAssembly = "Cannot extract GUID from an assembly";

		//<summary>
		//  Event which is triggered when a plug-in was installed.
		//</summary>
		public event EventHandler Installed;

		//<summary>
		//  Event which is triggered when a plug-in was uninstalled.
		//</summary>
		public event EventHandler Uninstalled;

		//<summary>
		//  Event which is triggered when an error is thrown during the plug-in manipulation (installation, uninstallation).
		//</summary>
		public event EventHandler<PluginErrorEventArgs> Error;

		IInstallablePluginContainer _pluginContainer;

		public void Initialize( string pluginFolderPath, PluginType pluginType, string fileFilter )
		{
			// Plug-in containers have to be initialized on the other domain in order to enable "on the fly" plug-in swapping.
			switch ( pluginType )
			{
				case PluginType.Persistence:
					SafeContainerCreation( () => _pluginContainer = new PersistenceProvider( pluginFolderPath ) );
					break;
				case PluginType.Interruption:
					SafeContainerCreation( () => _pluginContainer = new InterruptionAggregator( pluginFolderPath ) );
					break;
				case PluginType.Vdm:
					SafeContainerCreation( () => _pluginContainer = new LoadedSettings( pluginFolderPath ) );
					break;
			}

			var watcher = new PluginWatcher( pluginFolderPath, fileFilter );
			watcher.Created += ( sender, args ) => ExecuteSta( () => InstallPugin( GetGuidDromAssembly( args.FullPath ), args.FullPath, true ) );
			watcher.Deleted += ( sender, args ) => ExecuteSta( () => UninstallPugin( GetGuidDromAssembly( args.FullPath ), args.FullPath ) );
			watcher.Error += ( sender, args ) => Error( watcher, new PluginErrorEventArgs( new Exception( WatcherErrorMessage ), null ) );
		}

		/// <summary>
		///  Starts a new thread which is able to invoke GUI elements.
		/// </summary>
		static void ExecuteSta( Action action )
		{
			var thread = new Thread( o => action() );
			thread.SetApartmentState( ApartmentState.STA );
			thread.Start();
		}

		// TODO: Test if assembly loading is safe. Otherwise we should extract GUID from file name (bad idea but working for now).
		static Guid GetGuidFromName( string name )
		{
			try
			{
				var guid = name.Substring( 0, name.LastIndexOf( '.' ) );
				return new Guid( guid.Split( '\\' ).Last() );
			}
			catch ( Exception )
			{
				return new Guid();
			}
		}

		// TODO: Is assembly loading on different domain safe?
		static Guid GetGuidDromAssembly( string path )
		{
			try
			{
				var assembly = Assembly.LoadFrom( path );
				return new AssemblyInfo( assembly ).Guid;
			}
			catch ( Exception exception )
			{
				Console.WriteLine( CannotExtractFromAssembly + exception.Message );
				return GetGuidFromName( path );
			}
		}

		void SafeContainerCreation( Action action )
		{
			try
			{
				action();
			}
			catch ( CompositionException exception )
			{
				Error( _pluginContainer, new PluginErrorEventArgs( exception, null ) );
				throw;
			}
		}

		public int CompareVersion( Guid guid, Version version )
		{
			var installedPluginVersion = _pluginContainer.GetPluginVersion( guid );
			return version.CompareTo( installedPluginVersion );
		}

		public bool IsInstalled( Guid guid )
		{
			return _pluginContainer.GetPluginVersion( guid ) != null;
		}

		void InstallPugin( Guid guid, string pluginPath, bool refresh )
		{
			if ( refresh )
			{
				_pluginContainer.Refresh();
			}
			var installablePlugin = _pluginContainer.GetInstallablePlugin( guid );
			if ( installablePlugin != null )
			{
				if ( installablePlugin.Install( pluginPath ) )
				{
					Installed( this, new EventArgs() );
				}
				else
				{
					Error( this, new PluginErrorEventArgs( new Exception( InstalledErrorMessage ), installablePlugin.AssemblyInfo ) );
				}
			}
			else
			{
				Installed( this, new EventArgs() );
			}
		}

		void UninstallPugin( Guid guid, string pluginPath )
		{
			var installablePlugin = _pluginContainer.GetInstallablePlugin( guid );
			if ( installablePlugin != null )
			{
				if ( installablePlugin.Unistall( pluginPath ) )
				{
					_pluginContainer.Refresh();
					Uninstalled( this, new EventArgs() );
				}
				else
				{
					Error( this, new PluginErrorEventArgs( new Exception( UninstalledErrorMessage ), installablePlugin.AssemblyInfo ) );
				}
			}
			else
			{
				_pluginContainer.Refresh();
				Uninstalled( this, new EventArgs() );
			}
		}

		// TODO: Does plug-in configuration require more/different steps than installation?
		public void Configure( Guid guid, string pluginPath )
		{
			InstallPugin( guid, pluginPath, false );
		}

		public string GetPluginPath( Guid guid )
		{
			return _pluginContainer.GetPluginPath( guid );
		}

		public bool IsPluginConfigurable( Guid guid )
		{
			return _pluginContainer.GetInstallablePlugin( guid ) != null;
		}

		public void Dispose()
		{
			var provider = _pluginContainer as PersistenceProvider;
			if ( provider != null )
			{
				provider.Dispose();
			}
		}

		[SecurityPermission( SecurityAction.Demand, Flags = SecurityPermissionFlag.Infrastructure )]
		public override object InitializeLifetimeService()
		{
			return null;
		}
	}
}