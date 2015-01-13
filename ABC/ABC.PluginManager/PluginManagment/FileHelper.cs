using System.IO;
using System.Reflection;


namespace PluginManager.PluginManagment
{
	class FileHelper
	{
		public static string GetDllVersion( string dllPath )
		{
			return new AssemblyInfo( Assembly.LoadFile( dllPath ) ).Version;
		}

		public static string GetLastWriteDate( string dllPath )
		{
			return new FileInfo( dllPath ).LastWriteTime.ToShortDateString();
		}
	}
}