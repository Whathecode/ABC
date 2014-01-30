using System.Collections.Generic;
using System.Linq;
using ABC.Common;
using ABC.PInvoke.Process;
using Whathecode.System;


namespace ABC.Applications.Persistence
{
	/// <summary>
	///   Provides persistence providers for supported applications.
	/// </summary>
	public abstract class AbstractPersistenceProvider : AbstractDisposable
	{
		readonly ProcessTracker _processTracker;
		readonly Dictionary<int, string> _commandLine = new Dictionary<int, string>();


		protected AbstractPersistenceProvider()
		{
			// Set up the process tracker which checks with which command line arguments processes were launched.
			_processTracker = new ProcessTracker();
			_processTracker.ProcessStarted += p =>
			{
				_commandLine[ p.Id ] = p.CommandLine;
			};
			_processTracker.ProcessStopped += p => _commandLine.Remove( p.Id );
			_processTracker.Start();
		}


		public List<PersistedApplication> Suspend( List<IWindow> windows )
		{
			var persistedApplications = (
				from processWindows in windows.GroupBy( w => w.GetProcess() )
				let process = processWindows.Key
				let applicationPath = process.Modules[ 0 ].FileName
				let persistor = GetPersistenceProviders().FirstOrDefault( p => p.ProcessName == process.ProcessName )
				where persistor != null
				let persistedData = persistor.Suspend( process, _commandLine.FirstOrDefault( a => a.Key == process.Id ).Value )
				select
					new PersistedApplication( processWindows.ToList(), persistedData )
					{
						ApplicationPath = applicationPath,
						Persistor = persistor.GetType().AssemblyQualifiedName
					} );

			return persistedApplications.ToList();
		}

		public void Resume( List<PersistedApplication> persistedStates )
		{
			foreach ( var s in persistedStates )
			{
				AbstractApplicationPersistence persistor = GetPersistenceProviders().FirstOrDefault( p => p.GetType().AssemblyQualifiedName == s.Persistor );
				if ( persistor != null )
				{
					persistor.Resume( s.ApplicationPath, s.Data );
				}
			}
		}

		protected override void FreeManagedResources()
		{
			_processTracker.Dispose();
		}

		protected override void FreeUnmanagedResources()
		{
			// Nothing to do.
		}


		protected abstract List<AbstractApplicationPersistence> GetPersistenceProviders();
	}
}
