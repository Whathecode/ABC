using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;


namespace ABC.Plugins
{
	/// <summary>
	///   Helper class to help with MEF composition.
	/// </summary>
	public static class CompositionHelper
	{
		static CompositionHelper()
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
		/// <param name = "pluginFolderPath">The path where plugin assemblies are located.</param>
		public static CompositionContainer ComposeFromPath( object toCompose, string pluginFolderPath )
		{
			// Set up plugin container.
			if ( !Directory.Exists( pluginFolderPath ) )
			{
				Directory.CreateDirectory( pluginFolderPath );
			}
			var catalog = new DirectoryCatalog( pluginFolderPath );
			var container = new CompositionContainer( catalog );
			container.ComposeParts( toCompose );

			return container;
		}
	}
}
