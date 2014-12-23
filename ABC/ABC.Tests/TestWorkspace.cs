namespace ABC.Tests
{
	class TestWorkspace : AbstractWorkspace<TestSession>
	{
		public override TestSession Store()
		{
			return new TestSession();
		}
	}
}
