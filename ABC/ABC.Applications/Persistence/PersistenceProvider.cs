using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.Linq;
using ABC.Common;
using ABC.PInvoke.Process;
using Whathecode.System;
using Whathecode.System.Windows.Interop;


namespace ABC.Applications.Persistence
{
	/// <summary>
	///   Provides persistence providers for supported applications.
	/// </summary>
	public class PersistenceProvider : AbstractDisposable
	{
		readonly CompositionContainer _pluginContainer;
		readonly ProcessTracker _processTracker;
		readonly Dictionary<int, string> _commandLine = new Dictionary<int, string>();

		[ImportMany]
		readonly List<AbstractApplicationPersistence> _persistenceProviders = new List<AbstractApplicationPersistence>();


		public PersistenceProvider( string pluginFolderPath )
		{
			_pluginContainer = CompositionHelper.Compose( this, pluginFolderPath );

			// Set up the process tracker which checks with which command line arguments processes were launched.
			_processTracker = new ProcessTracker();
			_processTracker.ProcessStarted += p =>
			{
				_commandLine[ p.Id ] = p.CommandLine;
			};
			_processTracker.ProcessStopped += p => _commandLine.Remove( p.Id );
			_processTracker.Start();
		}


		public List<PersistedApplication> Suspend( List<WindowInfo> windows )
		{
			var persistedApplications = (
				from processWindows in windows.GroupBy( w => w.GetProcess() )
				let process = processWindows.Key
				let persistor = _persistenceProviders.FirstOrDefault( p => p.ProcessName == process.ProcessName )
				where persistor != null
				let persistedData = persistor.Suspend( process, _commandLine.FirstOrDefault( a => a.Key == process.Id ).Value )
				select new PersistedApplication( processWindows.ToList(), persistedData ) );

			return persistedApplications.ToList();
		}

		protected override void FreeManagedResources()
		{
			_processTracker.Dispose();
		}

		protected override void FreeUnmanagedResources()
		{
			// Nothing to do.
		}
	}
}
