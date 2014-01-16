namespace ABC.PInvoke.Process
{
	public class ProcessInfo
	{
		public int Id { get; private set; }
		public string Name { get; private set; }
		public string CommandLine { get; private set; }


		public ProcessInfo( int id, string name, string commandLine )
		{
			Id = id;
			Name = name;
			CommandLine = commandLine;
		}
	}
}
