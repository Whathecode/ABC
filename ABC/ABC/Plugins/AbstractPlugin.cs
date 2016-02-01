using System;
using System.Linq;
using System.Runtime.InteropServices;
using Whathecode.System;
using Whathecode.System.Reflection.Extensions;


namespace ABC.Plugins
{
	/// <summary>
	///   Base class for plugins providing integration with external applications which can be loaded by the ABC toolkit.
	/// </summary>
	public class AbstractPlugin
	{
		public AbstractPlugin()
		{
			Type pluginType = GetType();
			GuidAttribute attribute = pluginType.GetAttributes<GuidAttribute>().FirstOrDefault();

			// Verify whether a unique identifier for the plugin is specified.
			if ( attribute == null || new Guid( attribute.Value ) == Guid.Empty )
			{
				string error = $"An ABC plugin needs to have a unique ID, achieved by applying a {nameof( GuidAttribute )} to the plugin class.";
				throw new InvalidImplementationException( error );
			}
		}
	}
}
