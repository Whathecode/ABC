using System.IO;
using System.Reflection;


namespace PluginManager.PluginManagment
{
	class FileHelper
	{
		public static string GetDllVersion( string dllPath )
		{
			return dllPath != null ? new AssemblyInfo( Assembly.LoadFile( dllPath ) ).Version : null;
		}

		public static string GetLastWriteDate( string dllPath )
		{
			return dllPath != null ? new FileInfo( dllPath ).LastWriteTime.ToShortDateString() : null;
		}
	}
}