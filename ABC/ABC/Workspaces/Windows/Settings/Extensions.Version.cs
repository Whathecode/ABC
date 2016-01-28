using System;
using System.Diagnostics.Contracts;
using System.Linq;


namespace ABC.Workspaces.Windows.Settings
{
	static class Extensions
	{
		/// <summary>
		///   Determines whether the version number matches the version <see cref="pattern" />.
		/// </summary>
		/// <param name = "source">A version number.</param>
		/// <param name = "pattern">A string indicating a (possibly incomplete) version number.</param>
		/// <returns>
		///   True when the version number is an 'underlying' version of the specified pattern. E.g. the pattern "1", matches with "1.2.4".
		/// </returns>
		public static bool Matches( this Version source, string pattern )
		{
			// Parse and verify correct pattern.
			const string incorrectPattern = "Incorrect version pattern. Expecting e.g., \"1.2\". Max four numbers, minimum 1.";
			int[] patternVersion;
			try
			{
				 patternVersion = pattern.Split( '.' ).Select( Int32.Parse ).ToArray();
			}
			catch ( FormatException )
			{
				throw new ArgumentException( incorrectPattern, nameof( pattern ) );
			}
			if ( patternVersion.Length > 4 )
			{
				throw new ArgumentException( incorrectPattern, nameof( pattern ) );
			}

			if ( patternVersion[ 0 ] != source.Major )
			{
				return false;
			}
			if ( patternVersion.Length > 1 && patternVersion[ 1 ] != source.Minor )
			{
				return false;
			}
			if ( patternVersion.Length > 2 && patternVersion[ 2 ] != source.Build )
			{
				return false;
			}
			if ( patternVersion.Length > 3 && patternVersion[ 3 ] != source.Revision )
			{
				return false;
			}
			return true;
		}
	}
}
