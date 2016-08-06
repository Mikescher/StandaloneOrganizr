using System;
using System.Threading;
using System.Threading.Tasks;

namespace StandaloneOrganizr.WPF
{
	public class BackgroundQueue
	{
		public static BackgroundQueue Inst = new BackgroundQueue();

		private Task previousTask = Task.FromResult(true);
		private readonly object key = new object();

		private BackgroundQueue()
		{
			
		}

		public Task QueueTask(Action action)
		{
			lock (key)
			{
				previousTask = previousTask.ContinueWith(t => action()
					, CancellationToken.None
					, TaskContinuationOptions.None
					, TaskScheduler.Default);
				return previousTask;
			}
		}

		public Task<T> QueueTask<T>(Func<T> work)
		{
			lock (key)
			{
				var task = previousTask.ContinueWith(t => work()
					, CancellationToken.None
					, TaskContinuationOptions.None
					, TaskScheduler.Default);
				previousTask = task;
				return task;
			}
		}
	}
}
