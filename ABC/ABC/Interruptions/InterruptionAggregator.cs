using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using ABC.Plugins;


namespace ABC.Interruptions
{
	/// <summary>
	///   Aggregates interruptions raised by externally loaded plug-ins.
	/// </summary>
	public class InterruptionAggregator : AbstractInterruptionTrigger, IInstallablePluginContainer
	{
		CompositionContainer _pluginContainer;
		readonly DirectoryCatalog _pluginCatalog;

		[ImportMany]
		readonly List<AbstractInterruptionTrigger> _interruptionTriggers =
			new List<AbstractInterruptionTrigger>();


		public string PluginFolderPath
		{
			get { return _pluginCatalog.FullPath; }
		}

		public InterruptionAggregator( string pluginFolderPath )
			: base( Assembly.GetExecutingAssembly() )
		{
			_pluginCatalog = CompositionHelper.CreateDirectory( pluginFolderPath );
			_pluginContainer = CompositionHelper.ComposeFromPath( this, _pluginCatalog );

			// Initialize loaded interruption handlers.
			foreach ( var handler in _interruptionTriggers )
			{
				handler.InterruptionReceived -= TriggerInterruption;
				handler.InterruptionReceived += TriggerInterruption;
			}
		}

		public void Refresh()
		{
			_pluginCatalog.Refresh();
		}

		public override void Update( DateTime now )
		{
			if ( !Monitor.TryEnter( this ) )
			{
				return;
			}

			foreach ( var trigger in _interruptionTriggers )
			{
				PluginHelper<AbstractInterruptionTrigger>.SafePluginInvoke( trigger, t => t.Update( now ) );
			}

			Monitor.Exit( this );
		}

		/// <summary>
		///   The <see cref = "AbstractInterruptionTrigger.InterruptionReceived" /> event returns interruption types which are serializable.
		///   In order to serialize them however, a <see cref = "DataContractSerializer" /> needs to be aware of the exact types.
		///   These are returned by this function.
		/// </summary>
		/// <returns>All the interruption types this interruption aggregator knows about.</returns>
		public override List<Type> GetInterruptionTypes()
		{
			return _interruptionTriggers
				.SelectMany( h => PluginHelper<AbstractInterruptionTrigger>.SafePluginInvoke( h, t => t.GetInterruptionTypes() ) )
				.ToList();
		}

		AbstractInterruptionTrigger GetInterruptionTrigger( Guid guid )
		{
			return _interruptionTriggers
				.FirstOrDefault( interruption => interruption.AssemblyInfo.Guid == guid );
		}

		public Version GetPluginVersion( Guid guid )
		{
			var plugin = GetInterruptionTrigger( guid );
			return plugin != null ? plugin.AssemblyInfo.Version : null;
		}

		public IInstallable GetInstallablePlugin( Guid guid )
		{
			return GetInterruptionTrigger( guid ) as IInstallable;
		}
	}
}