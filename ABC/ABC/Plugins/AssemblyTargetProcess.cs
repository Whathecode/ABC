using System;


namespace ABC.Plugins
{
	[AttributeUsage( AttributeTargets.Assembly )]
	public class AssemblyTargetProcess : Attribute
	{
		public AssemblyTargetProcess()
			: this( string.Empty, string.Empty, string.Empty ) {}

		public AssemblyTargetProcess( string name, string companyName, string version )
		{
			Name = name;
			CompanyName = companyName;
			Version = new Version( version );
		}

		public string Name { get; private set; }
		public string CompanyName { get; private set; }
		public Version Version { get; private set; }
	}
}