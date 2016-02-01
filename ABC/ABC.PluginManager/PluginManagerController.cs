using System;
using System.Collections.Generic;
using System.Security.Permissions;
using System.Windows;
using ABC.Plugins;
using PluginManager.Model;
using PluginManager.PluginManagement;


namespace PluginManager
{
	class PluginManagerController : MarshalByRefObject, IDisposable
	{
		public event EventHandler RefreshingEvent;
		public PluginProvider PersistenceContainer { get; private set; }

		public PluginProvider InterruptionsContainer { get; private set; }

		public PluginProvider VdmContainer { get; private set; }

		public Dictionary<PluginType, PluginProvider> ProviderDictionary { get; private set; }


		public PluginManagerController()
		{
			PersistenceContainer = ShadowCopyFactory<PluginProvider>.Create( App.PersistencePluginLibrary );
			PersistenceContainer.Initialize( App.PersistencePluginLibrary, PluginType.Persistence, "*.dll" );
			AttachEvents( PersistenceContainer );

			InterruptionsContainer = ShadowCopyFactory<PluginProvider>.Create( App.InterruptionsPluginLibrary );
			InterruptionsContainer.Initialize( App.InterruptionsPluginLibrary, PluginType.Interruption, "*.dll" );
			AttachEvents( InterruptionsContainer );

			VdmContainer = ShadowCopyFactory<PluginProvider>.Create( App.VdmPluginLibrary );
			VdmContainer.Initialize( App.VdmPluginLibrary, PluginType.Vdm, "*.xml" );
			AttachEvents( VdmContainer );

			ProviderDictionary = new Dictionary<PluginType, PluginProvider>
			{
				{ PluginType.Persistence, PersistenceContainer },
				{ PluginType.Interruption, InterruptionsContainer },
				{ PluginType.Vdm, VdmContainer }
			};
		}

		void AttachEvents( PluginProvider pluginProvider )
		{
			// Connection to plug-ins providers (MarshalByRefObject has to be implemented).
			pluginProvider.Installed += ( sender, args ) => RefreshingEvent( this, new EventArgs() );
			pluginProvider.Uninstalled += ( sender, args ) => RefreshingEvent( this, new EventArgs() );
			PersistenceContainer.Error += ( sender, args ) => MessageBox.Show( args.GetException().Message );
		}

		public void Dispose()
		{
			if ( PersistenceContainer != null )
				PersistenceContainer.Dispose();
		}

		public void Configure( Guid guid, PluginType type, string pluginPath )
		{
			var pluginContainer = type == PluginType.Persistence ? PersistenceContainer : type == PluginType.Interruption ? InterruptionsContainer : VdmContainer;
			pluginContainer.Configure( guid, pluginPath );
		}

		[SecurityPermission( SecurityAction.Demand, Flags = SecurityPermissionFlag.Infrastructure )]
		public override object InitializeLifetimeService()
		{
			// Guarantee infinite lifetime for remoting.
			return null;
		}
	}
}