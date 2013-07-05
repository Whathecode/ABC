using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.IO;
using System.Threading;


namespace ABC.Interruptions
{
	/// <summary>
	///   Aggregates interruptions raised by externally loaded plugins.
	/// </summary>
	public class InterruptionAggregator : AbstractInterruptionHandler
	{
		readonly CompositionContainer _pluginContainer;

		[ImportMany]
		readonly List<AbstractInterruptionHandler> _interruptionHandlers = new List<AbstractInterruptionHandler>();


		public InterruptionAggregator( string pluginFolderPath )
		{
			// Set up plugin container.
			if ( !Directory.Exists( pluginFolderPath ) )
			{
				Directory.CreateDirectory( pluginFolderPath );
			}
			var catalog = new DirectoryCatalog( pluginFolderPath );
			_pluginContainer = new CompositionContainer( catalog );
			_pluginContainer.ComposeParts( this );

			// Initialize loaded interruption handlers.
			foreach ( var handler in _interruptionHandlers )
			{
				handler.InterruptionReceived += TriggerInterruption;
			}
		}


		public override void Update( DateTime now )
		{
			if ( !Monitor.TryEnter( this ) )
			{
				return;
			}

			foreach ( var handler in _interruptionHandlers )
			{
				// ReSharper disable EmptyGeneralCatchClause
				try
				{
					handler.Update( now );
				}
				catch ( Exception ex )
				{
					// Prevent main application from crashing when plugins throw exceptions.
					string pluginName = handler.GetType().ToString();
					Debug.WriteLine( "The interruption handler plugin \"{0}\" threw an exception:\n {1}", pluginName, ex );
				}
				// ReSharper restore EmptyGeneralCatchClause
			}

			Monitor.Exit( this );
		}
	}
}
