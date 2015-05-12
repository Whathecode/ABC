using System;
using System.ComponentModel.Composition;
using System.Reflection;
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

		//<summary>
		//  Event which is triggered when plug-in was installed.
		//</summary>
		public event EventHandler<PluginEventArgs> Installed;

		//<summary>
		//  Event which is triggered when plug-in was uninstalled.
		//</summary>
		public event EventHandler<PluginEventArgs> Uninstalled;

		//<summary>
		//  Event which is triggered when error is throw during plug-in detection, installation/uninstallation or composition.
		//</summary>
		public event EventHandler<PluginErrorEventArgs> Error;

		IInstallablePluginContainer _pluginContainer;

		public void Initialize( string pluginFolderPath, PluginType pluginType, string fileFilter )
		{
			// Plug-in containers have to be initialized inside of object which derives from MarshalByRefObject in order
			// to be on separate domain ( plug-ins on the fly swapping).
			switch ( pluginType )
			{
				case PluginType.Persistence:
					SafeContainerCreation( () => _pluginContainer = new PersistenceProvider( pluginFolderPath ) );
					break;
				case PluginType.Interruption:
					SafeContainerCreation( () => _pluginContainer = new InterruptionAggregator( pluginFolderPath ) );
					break;
				case PluginType.Vdm:
					SafeContainerCreation( () => _pluginContainer = new LoadedSettings( false, false, null, pluginFolderPath ) );
					break;
			}

			var watcher =  new PluginWatcher( _pluginContainer.PluginFolderPath, fileFilter );
			watcher.Created += ( sender, args ) => InstallPugin( new AssemblyInfo(Assembly.LoadFile( args.FullPath )).Guid );
			watcher.Deleted += ( sender, args ) => UninstallPugin( new AssemblyInfo(Assembly.LoadFile( args.FullPath )).Guid );
			watcher.Error += ( sender, args ) => Error( watcher, new PluginErrorEventArgs( new Exception( WatcherErrorMessage ), null ) );
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

		void InstallPugin( Guid guid )
		{
			_pluginContainer.Refresh();
			var installablePlugin = _pluginContainer.GetInstallablePlugin( guid );
			if ( installablePlugin != null && installablePlugin.Install() )
			{
				Installed( this, new PluginEventArgs( installablePlugin.AssemblyInfo ) );
			}
			else if ( installablePlugin != null )
			{
				Error( this, new PluginErrorEventArgs( new Exception( InstalledErrorMessage ), installablePlugin.AssemblyInfo ) );
			}
		}

		void UninstallPugin( Guid guid )
		{
			var installablePlugin = _pluginContainer.GetInstallablePlugin( guid );
			if ( installablePlugin != null && installablePlugin.Unistall() )
			{
				Uninstalled( this, new PluginEventArgs( installablePlugin.AssemblyInfo ) );
			}
			else if ( installablePlugin != null )
			{
				Error( this, new PluginErrorEventArgs( new Exception( UninstalledErrorMessage ), installablePlugin.AssemblyInfo ) );
			}
			_pluginContainer.Refresh();
		}

		public void Dispose()
		{
			var provider = _pluginContainer as PersistenceProvider;
			if ( provider != null )
			{
				provider.Dispose();
			}
		}
	}
}