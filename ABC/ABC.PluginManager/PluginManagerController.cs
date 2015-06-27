using System;
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


		public PluginManagerController()
		{
			PersistenceContainer = PluginProviderFactory.CreateProvider( App.PersistencePluginLibrary, PluginType.Persistence, "*.dll" );
			PersistenceContainer.Installed += ( sender, args ) => RefreshingEvent( this, new EventArgs() );
			PersistenceContainer.Uninstalled += ( sender, args ) => RefreshingEvent( this, new EventArgs() );
			PersistenceContainer.Error += ( sender, args ) => MessageBox.Show( "Persistence composition error", args.ToString() );

			InterruptionsContainer = PluginProviderFactory.CreateProvider( App.InterruptionsPluginLibrary, PluginType.Interruption, "*.dll" );
			InterruptionsContainer.Installed += ( sender, args ) => RefreshingEvent( this, new EventArgs() );
			InterruptionsContainer.Uninstalled += ( sender, args ) => RefreshingEvent( this, new EventArgs() );
			InterruptionsContainer.Error += ( sender, args ) => MessageBox.Show( "Interruptions composition error", args.ToString() );

			VdmContainer = PluginProviderFactory.CreateProvider( App.VdmPluginLibrary, PluginType.Vdm, "*.xml" );
			VdmContainer.Installed += ( sender, args ) => RefreshingEvent( this, new EventArgs() );
			VdmContainer.Uninstalled += ( sender, args ) => RefreshingEvent( this, new EventArgs() );
			VdmContainer.Error += ( sender, args ) => MessageBox.Show( "Persistence composition error", args.ToString() );
		}

		public void Dispose()
		{
			if ( PersistenceContainer != null )
				PersistenceContainer.Dispose();
		}
	}
}