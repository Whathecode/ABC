using System;
using System.ComponentModel.Composition;
using ABC.Applications.Persistence;
using ABC.Plugins;
using Whathecode.System;


namespace PluginManager.PluginManagment
{
	public class InstalledPluginContainer : AbstractDisposable
	{
		public delegate void PluginManagerEventHandler( string message, Guid pluginGuid );

		/// <summary>
		///   Event which is triggered when plug-in cannot be loaded.
		/// </summary>
		public event PluginManagerEventHandler PluginCompositionFailEvent;

		/// <summary>
		///   Event which is triggered when plug-in installation processes has ended.
		/// </summary>
		public event PluginManagerEventHandler PluginInstalledEvent;

		readonly IInstallablePluginContainer _pluginContainer;

		public const string CompositionFailMessage = "Plug-in manager was not initialized properly. Please check installed plug-ins and restart manager.";
		const string InstalledMessage = "Plug-in was installed correctly.";
		const string NotInstalledMessage = "Plug-in was not installed correctly.";

		public InstalledPluginContainer( IInstallablePluginContainer pluginContainer )
		{
			_pluginContainer = pluginContainer;
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


		public void InstallPlugin( Guid guid )
		{
			if ( !IsInitialized() )
			{
				return;
			}
			try
			{
				_pluginContainer.Reload();
				_pluginContainer.InstallPugin( guid );
			}
			catch ( CompositionException exception )
			{
				PluginCompositionFailEvent( exception.RootCauses[ 0 ].Message + "\n" + CompositionFailMessage, Guid.Empty );
			}

			PluginInstalledEvent( _pluginContainer.InstallPugin( guid ) ? InstalledMessage : NotInstalledMessage, guid );
		}

		bool IsInitialized()
		{
			if ( _pluginContainer != null )
			{
				return true;
			}

			PluginCompositionFailEvent( CompositionFailMessage, Guid.Empty );
			return false;
		}

		protected override void FreeManagedResources()
		{
			var provider = _pluginContainer as PersistenceProvider;
			if ( provider != null )
			{
				provider.Dispose();
			}
		}

		protected override void FreeUnmanagedResources()
		{
			// Nothing to do.
		}
	}
}