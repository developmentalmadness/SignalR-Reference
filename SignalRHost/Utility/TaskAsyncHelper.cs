using System.Threading.Tasks;

namespace SignalRHost.Utility
{
	public static class TaskAsyncHelper
	{
		private static readonly Task emptyTask = MakeTask<object>(null);

		private static Task<T> MakeTask<T>(T value)
		{
			return FromResult<T>(value);
		}

		public static Task<T> FromResult<T>(T value)
		{
			var tcs = new TaskCompletionSource<T>();
			tcs.SetResult(value);
			return tcs.Task;
		}

		public static Task Empty
		{
			get
			{
				return emptyTask;
			}
		}
	}
}
