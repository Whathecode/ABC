using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;


namespace ABC.Workspaces.Library
{
	/// <summary>
	///   Allows storing the state of a <see cref="Library" />.
	/// </summary>
	[DataContract]
	public class StoredLibrary
	{
		[DataMember]
		readonly List<string> _paths;
		internal IReadOnlyCollection<string> Paths { get { return _paths; } }


		public StoredLibrary( Library library )
		{
			_paths = library.Paths.ToList();
		}
	}
}
