using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
			// Before importing any plugins, make sure that all loaded assemblies can be resolved correctly.
			// Otherwise required DLLs for the plugin need to be located in the executable folder and vice versa.
			// For more information: http://stackoverflow.com/a/6646769/590790
			AppDomain.CurrentDomain.AssemblyResolve += ( o, args ) =>
			{
				var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
				return loadedAssemblies.FirstOrDefault( a => a.FullName == args.Name );
			};

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
