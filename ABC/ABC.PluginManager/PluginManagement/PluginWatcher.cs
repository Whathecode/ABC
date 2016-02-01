using System.IO;

namespace PluginManager.PluginManagement
{
	public class PluginWatcher
	{
		readonly FileSystemWatcher _fileSystemWatcher;

		public PluginWatcher( string path, string filter = null )
		{
			_fileSystemWatcher = new FileSystemWatcher( path )
			{
				NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.Size | NotifyFilters.LastWrite
				               | NotifyFilters.FileName | NotifyFilters.Attributes | NotifyFilters.Security,
				EnableRaisingEvents = true
			};
			if ( filter != null )
				_fileSystemWatcher.Filter = filter;
		}

		public event ErrorEventHandler Error
		{
			add { _fileSystemWatcher.Error += value; }
			remove { _fileSystemWatcher.Error -= value; }
		}

		FileSystemEventHandler _createdHandler;
		public event FileSystemEventHandler Created
		{
			add
			{
				_createdHandler = ( sender, args ) => SafeEventHandle( value, sender, args );
				_fileSystemWatcher.Created += _createdHandler;
			}
			remove
			{
				if (_createdHandler != null)
				_fileSystemWatcher.Created -= _createdHandler;
			}
		}

		FileSystemEventHandler _deletedHandler;
		public event FileSystemEventHandler Deleted
		{
			add
			{
				_deletedHandler = ( sender, args ) => SafeEventHandle( value, sender, args );
				_fileSystemWatcher.Deleted += _deletedHandler;
			}
			remove
			{
				if (_deletedHandler != null)
				_fileSystemWatcher.Deleted -= _deletedHandler;
			}
		}

		FileSystemEventHandler _changedHandler;
		public event FileSystemEventHandler Changed
		{
			add
			{
				_changedHandler = ( sender, args ) => SafeEventHandle( value, sender, args );
				_fileSystemWatcher.Deleted += _changedHandler;
			}
			remove
			{
				if (_changedHandler != null)
				_fileSystemWatcher.Deleted -= _changedHandler;
			}
		}

		void SafeEventHandle( FileSystemEventHandler pluginEvent, object sender, FileSystemEventArgs e )
		{
			if ( pluginEvent == null )
				return;

			// Hack to avoid handling multiple events. More on:
			// http://stackoverflow.com/questions/1764809/filesystemwatcher-changed-event-is-raised-twice
			_fileSystemWatcher.EnableRaisingEvents = false;
			pluginEvent.Invoke( sender, e );
			_fileSystemWatcher.EnableRaisingEvents = true;
		}
	}
}
