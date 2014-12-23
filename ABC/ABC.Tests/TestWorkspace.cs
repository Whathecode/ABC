namespace ABC.Tests
{
	class TestWorkspace : IWorkspace
	{
		public object Store()
		{
			return new TestSession();
		}
	}
}
