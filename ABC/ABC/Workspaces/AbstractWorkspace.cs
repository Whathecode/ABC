using System;
using System.ComponentModel;
using System.Threading;


namespace ABC.Workspaces
{
	/// <summary>
	///   A workspace which is managed by an <see cref = "AbstractWorkspaceManager{TWorkspace, TSession}" />.
	/// </summary>
	/// <typeparam name = "TSession">The type which is used to serialize workspaces as a session.</typeparam>
	public abstract class AbstractWorkspace<TSession>
	{
		/// <summary>
		///   Event handler which reports on a certain event related to a given workspace.
		/// </summary>
		/// <param name = "workspace">The workspace the event relates to.</param>
		public delegate void WorkspaceEventHandler( AbstractWorkspace<TSession> workspace );


		bool _isSuspending;

		public bool IsVisible { get; private set; }

		public IWorkspace NonGeneric { get; private set; }

		/// <summary>
		///   Determines whether the workspace is currently in a suspended state,
		///   meaning all its resources have been closed and are ready to be stored.
		///   TODO: Make setter private and store IsSuspended through this class instead?
		/// </summary>
		public bool IsSuspended { get; protected set; }

		/// <summary>
		///   Event which is triggered when suspension of a workspace has started.
		/// </summary>
		public event WorkspaceEventHandler SuspendingWorkspace = delegate { }; 

		/// <summary>
		///   Event which is triggered after the workspace has been suspended and no longer contains any resources.
		/// </summary>
		public event WorkspaceEventHandler SuspendedWorkspace = delegate { };

		/// <summary>
		///   Event which is triggered when a forced suspension is requested by calling <see cref="ForceSuspend" />.
		/// </summary>
		public event WorkspaceEventHandler ForceSuspendRequested = delegate { };

		/// <summary>
		///   Event which is triggered whenever <see cref="Store" /> is called, prior to commencing the store operation.
		/// </summary>
		public event WorkspaceEventHandler Storing = delegate { }; 


		protected AbstractWorkspace()
		{
			NonGeneric = new NonGenericWorkspace<TSession>( this );
		}


		internal void Show()
		{
			ShowInner();
			IsVisible = true;
		}

		protected abstract void ShowInner();

		internal void Hide()
		{
			HideInner();
			IsVisible = false;
		}

		protected abstract void HideInner();

		/// <summary>
		///   Determine whether there are resources left that need suspension in case <see cref="Suspend" /> is called.
		/// </summary>
		public abstract bool HasResourcesToSuspend();

		/// <summary>
		///   Suspends this workspace asynchronously, waiting for all resources to be closed and ready to be stored.
		/// </summary>
		public void Suspend()
		{
			if ( _isSuspending || IsSuspended )
			{
				return;
			}

			SuspendingWorkspace( this );
			_isSuspending = true;

			// Initiate the actual suspension.
			// This needs to occur before waiting for suspension since aggregate and composite workspaces
			// need to notify their containing workspaces of suspension prior to checking whether all resources have been suspended.
			SuspendInner();

			// Await full suspension in background.
			var awaitSuspend = new BackgroundWorker();
			awaitSuspend.DoWork += ( sender, args ) =>
			{
				while ( HasResourcesToSuspend() )
				{
					Thread.Sleep( TimeSpan.FromSeconds( 1 ) );
				}
			};
			awaitSuspend.RunWorkerCompleted += ( sender, args ) =>
			{
				IsSuspended = true;
				_isSuspending = false;
				SuspendedWorkspace( this );
			};
			awaitSuspend.RunWorkerAsync();
		}

		/// <summary>
		///   Suspends this workspace by closing all resources and preparing them to be stored.
		/// </summary>
		protected abstract void SuspendInner();

		/// <summary>
		///   Suspend the workspace, regardless of whether resoures are still open.
		///   A workaround could be simply closing the resource.
		///   Whichever approach taken, <see cref="HasResourcesToSuspend" /> should return false.
		/// </summary>
		public void ForceSuspend()
		{
			// Suspend when not suspending yet.
			if ( !_isSuspending )
			{
				Suspend();
			}

			ForceSuspendRequested( this );
			ForceSuspendInner();
		}

		protected abstract void ForceSuspendInner();

		/// <summary>
		///   Resumes suspended resources which are held within the workspace.
		/// </summary>
		public void Resume()
		{
			if ( !IsSuspended )
			{
				return;
			}

			ResumeInner();

			IsSuspended = false;
		}

		/// <summary>
		///   Resumes suspended resources which are held within the workspace.
		/// </summary>
		protected abstract void ResumeInner();

		/// <summary>
		///   Serialize the current workspace to a structure, allowing to restore it at a later time.
		///   In case the workspace is suspended, suspended resources are stored for later resumption as well.
		/// </summary>
		/// <returns>A structure holding the relevant information for this workspace.</returns>
		public TSession Store()
		{
			Storing( this );

			return StoreInner();
		}

		/// <summary>
		///   Serialize the current workspace to a structure, allowing to restore it at a later time.
		///   In case the workspace is suspended, suspended resources are stored for later resumption as well.
		/// </summary>
		/// <returns>A structure holding the relevant information for this workspace.</returns>
		protected abstract TSession StoreInner();
	}
}
