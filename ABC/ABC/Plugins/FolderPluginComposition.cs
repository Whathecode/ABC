using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using Whathecode.System;


namespace ABC.Plugins
{
	/// <summary>
	///   Helper class to help with MEF composition with assemblies loaded from a directory.
	/// </summary>
	public class FolderPluginComposition : AbstractDisposable
	{
		readonly CompositionContainer _container;
		readonly DirectoryCatalog _catalog;

		/// <summary>
		///   The path where plugin assemblies are loaded from.
		/// </summary>
		public string Folder { get; }


		static FolderPluginComposition()
		{
			// Before importing any plugins, make sure that all loaded assemblies can be resolved correctly.
			// Otherwise required DLLs for the plugin need to be located in the executable folder and vice versa.
			// For more information: http://stackoverflow.com/a/6646769/590790
			AppDomain.CurrentDomain.AssemblyResolve += ( o, args ) =>
			{
				var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
				return loadedAssemblies.FirstOrDefault( a => a.FullName == args.Name );
			};
		}


		/// <summary>
		///   Create a composition container from assemblies located at a particular path.
		/// </summary>
		/// <param name = "toCompose">The object to compose a composition container for.</param>
		/// <param name = "folder">The path where plugin assemblies are located.</param>
		public FolderPluginComposition( object toCompose, string folder )
		{
			Folder = folder;

			if ( !Directory.Exists( Folder ) )
			{
				Directory.CreateDirectory( Folder );
			}
			_catalog = new DirectoryCatalog( Folder );
			_container = new CompositionContainer( _catalog );
			_container.ComposeParts( toCompose );
		}


		/// <summary>
		///   Reloads plugins from the directory associated to this object.
		/// </summary>
		public void Refresh()
		{
			_catalog.Refresh();
		}

		/// <summary>
		///   Gets the collection of assemblies currently loaded.
		/// </summary>
		public ReadOnlyCollection<string> GetLoadedFiles()
		{
			return _catalog.LoadedFiles;
		}

		protected override void FreeManagedResources()
		{
			_container.Dispose();
		}

		protected override void FreeUnmanagedResources()
		{
			// Nothing to do.
		}
	}
}
