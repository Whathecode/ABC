namespace ABC
{
	public interface IWorkspace
	{
		/// <summary>
		///   Serialize the current workspace to a structure, allowing to restore it at a later time.
		/// </summary>
		/// <returns>A structure holding the relevant information for this desktop.</returns>
		object Store();
	}
}
