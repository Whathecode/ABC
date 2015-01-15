using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using ABC.Common;


namespace ABC.Interruptions
{
	/// <summary>
	///   Aggregates interruptions raised by externally loaded plugins.
	/// </summary>
	public class InterruptionAggregator : AbstractInterruptionTrigger, IInstallablePluginContainer
	{
		CompositionContainer _pluginContainer;
		readonly string _pluginFolderPath;
		[ImportMany]
		readonly List<AbstractInterruptionTrigger> _interruptionTriggers = new List<AbstractInterruptionTrigger>();


		public InterruptionAggregator( string pluginFolderPath )
		{
			_pluginFolderPath = pluginFolderPath;
			Reload();
		}

		public void Reload()
		{
			_pluginContainer = CompositionHelper.ComposeFromPath( this, _pluginFolderPath );

			// Initialize loaded interruption handlers.
			foreach ( var handler in _interruptionTriggers )
			{
				handler.InterruptionReceived -= TriggerInterruption;
				handler.InterruptionReceived += TriggerInterruption;
			}
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

		public List<PluginInformation> GetPluginInformation()
		{
			var infos = new List<PluginInformation>();
			_interruptionTriggers.ForEach( it => infos.Add( it.Info ) );
			return infos;
		}

		public IInstallable GetInstallablePlugin( string name, string companyName )
		{
			return _interruptionTriggers
				.FirstOrDefault( interruption => interruption.Info.ProcessName == name && interruption.Info.CompanyName == companyName ) as IInstallable;
		}
	}
}
