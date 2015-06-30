using System;
using System.Collections.Generic;
using System.Windows;
using PluginManager.Model;
using PluginManager.PluginManagment;


namespace PluginManager
{
	[Serializable]
	class PluginManagerController : MarshalByRefObject, IDisposable
	{
		public event EventHandler RefreshingEvent;
		public PluginProvider PersistenceContainer { get; private set; }

		public PluginProvider InterruptionsContainer { get; private set; }

		public PluginProvider VdmContainer { get; private set; }

		public Dictionary<PluginType, PluginProvider> ProviderDictionary { get; private set; }


		public PluginManagerController()
		{
			PersistenceContainer = PluginProviderFactory.CreateProvider( App.PersistencePluginLibrary, PluginType.Persistence, "*.dll" );
			AttachEvents( PersistenceContainer );
			PersistenceContainer.Error += ( sender, args ) => MessageBox.Show( "Persistence composition error", args.ToString() );

			InterruptionsContainer = PluginProviderFactory.CreateProvider( App.InterruptionsPluginLibrary, PluginType.Interruption, "*.dll" );
			AttachEvents( InterruptionsContainer );
			PersistenceContainer.Error += ( sender, args ) => MessageBox.Show( "Interruptions composition error", args.ToString() );

			VdmContainer = PluginProviderFactory.CreateProvider( App.VdmPluginLibrary, PluginType.Vdm, "*.xml" );
			AttachEvents( VdmContainer );
			PersistenceContainer.Error += ( sender, args ) => MessageBox.Show( "VDM composition error", args.ToString() );

			ProviderDictionary = new Dictionary<PluginType, PluginProvider>
			{
				{ PluginType.Persistence, PersistenceContainer },
				{ PluginType.Interruption, InterruptionsContainer },
				{ PluginType.Vdm, VdmContainer }
			};
		}

		void AttachEvents( PluginProvider pluginProvider )
		{
			pluginProvider.Installed += ( sender, args ) => RefreshingEvent( this, new EventArgs() );
			pluginProvider.Uninstalled += ( sender, args ) => RefreshingEvent( this, new EventArgs() );
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
	}
}