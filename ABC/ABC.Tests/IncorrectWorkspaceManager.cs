namespace ABC.Tests
{
	class IncorrectWorkspaceManager : AbstractWorkspaceManager<TestWorkspace, TestSession>
	{
		protected override TestWorkspace CreateEmptyWorkspaceInner()
		{
			return new TestWorkspace();
		}

		protected override TestWorkspace CreateWorkspaceFromSessionInner( TestSession session )
		{
			return new TestWorkspace();
		}

		protected override void SwitchToWorkspaceInner( TestWorkspace workspace )
		{
		}

		protected override void MergeInner( TestWorkspace from, TestWorkspace to )
		{
		}

		protected override void CloseAdditional()
		{
		}

		protected override void FreeUnmanagedResources()
		{
		}
	}
}
