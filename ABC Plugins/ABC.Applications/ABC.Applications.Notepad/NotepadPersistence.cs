using System;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using ABC.Applications.Persistence;
using Whathecode.System.Diagnostics;


namespace ABC.Applications.Notepad
{
	[Guid( "EA671367-2C10-4DA1-8757-E2D753001812" )]
	[Export( typeof( AbstractApplicationPersistence ) )]
	public class NotepadPersistence : AbstractApplicationPersistence
	{
		public NotepadPersistence()
			: base( "notepad", "Microsoft Corporation" )
		{
		}


		public override object Suspend( SuspendInformation toSuspend )
		{
			// Check whether the necessary data to know which file was open is passed.
			if ( toSuspend.CommandLine == null )
			{
				return null;
			}

			// TODO: Should we distribute taskkill to ensure it being there?
			ProcessHelper.SetUp( @"C:\Windows\System32\taskkill.exe", "/pid " + toSuspend.Process.Id, "", true ).Run();

			// Extract file name from commandLine which is in format: "... path/to/notepad.exe" C:\Path\To\File\Without Quotes\text file.txt
			Match split = Regex.Match( toSuspend.CommandLine, "\"(.*)\" (.*)" );
			return split.Groups[ 2 ].Value;
		}

		public override void Resume( string applicationPath, object persistedData )
		{
			if ( persistedData == null )
			{
				return;
			}

			var filePath = (string)persistedData;

			ProcessHelper.SetUp( applicationPath, filePath ).Run();
		}

		public override Type GetPersistedDataType()
		{
			return typeof( string );
		}
	}
}