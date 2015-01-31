namespace ABC.Workspaces
{
	class TestWorkspace : AbstractWorkspace<TestSession>
	{
		protected override void ShowInner()
		{
			throw new System.NotImplementedException();
		}

		protected override void HideInner()
		{
			throw new System.NotImplementedException();
		}

		public override bool HasResourcesToSuspend()
		{
			return false;
		}

		protected override void ShowInner()
		{
			// Nothing to do.
		}

		protected override void HideInner()
		{
			// Nothing to do.
		}

		protected override void SuspendInner()
		{
			// Nothing to do.
		}

		protected override void ForceSuspendInner()
		{
			// Nothing to do.
		}

		protected override void ResumeInner()
		{
			// Nothing to do.
		}

		protected override TestSession StoreInner()
		{
			return new TestSession();
		}
	}
}
