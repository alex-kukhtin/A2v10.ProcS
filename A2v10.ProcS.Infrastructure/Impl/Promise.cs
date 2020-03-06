// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace A2v10.ProcS.Infrastructure
{
	public static class PromiseExtensions
	{
		public static Task WaitFor(this IPromise promise)
		{
			var tc = new TaskCompletionSource<Boolean>();
			promise.Done(() => tc.SetResult(true));
			promise.Catch(e => tc.SetException(e));
			return tc.Task;
		}

		public static Task WaitFor<T>(this IPromise<T> promise)
		{
			var tc = new TaskCompletionSource<T>();
			promise.Done(r => tc.SetResult(r));
			promise.Catch(e => tc.SetException(e));
			return tc.Task;
		}
	}

	public abstract class PromiseBase
	{
		protected Boolean isDone = false;
		protected Boolean isFailed = false;
		protected Action<Exception> onException = null;
		protected Exception exception = null;

		public void SignalException(Exception e)
		{
			lock (this)
			{
				if (isFailed) return;
				if (isDone) throw new Exception("Promise already done");
				exception = e;
				isFailed = true;
				onException?.Invoke(exception);
			}
		}

		protected Boolean SignalDoneCheck()
		{
			if (isDone) return false;
			if (isFailed) throw new Exception("Promise already failed");
			return true;
		}

		protected Boolean DoneThrow()
		{
			throw new Exception("Can't subscribe more than one catchers");
		}

		protected void BaseCatch(Action<Exception> action)
		{
			lock (this)
			{
				if (onException != null) throw new Exception("Can't subscribe more than one catchers");
				onException = action;
				if (isFailed) onException?.Invoke(exception);
			}
		}
	}

	public class Promise : PromiseBase, IPromise
	{
		protected Action onDone = null;

		public void SignalDone()
		{
			lock (this)
			{
				if (!SignalDoneCheck()) 
					return;
				isDone = true;
				onDone?.Invoke();
			}
		}

		public IPromise Done(Action action)
		{
			lock (this)
			{
				if (onDone != null) 
					DoneThrow();
				onDone = action;
				if (isDone) 
					onDone?.Invoke();
			}
			return this;
		}

		public IPromise Catch(Action<Exception> action)
		{
			BaseCatch(action);
			return this;
		}
	}

	public class Promise<T> : PromiseBase, IPromise<T>, IPromise
	{

		protected Action<T> onDone = null;
		protected T result;

		public void SignalDone(T res)
		{
			lock (this)
			{
				if (!SignalDoneCheck()) return;
				result = res;
				isDone = true;
				onDone?.Invoke(result);
			}
		}

		public IPromise<T> Done(Action<T> action)
		{
			lock (this)
			{
				if (onDone != null) DoneThrow();
				onDone = action;
				if (isDone) onDone?.Invoke(result);
			}
			return this;
		}

		public IPromise<T> Catch(Action<Exception> action)
		{
			BaseCatch(action);
			return this;
		}

		public IPromise Done(Action action)
		{
			lock (this)
			{
				onDone = t => action();
				if (isDone) onDone?.Invoke(result);
			}
			return this;
		}

		IPromise IPromise.Catch(Action<Exception> action)
		{
			BaseCatch(action);
			return this;
		}
	}

	public class PromiseAggregator
	{
		private readonly Task[] tasks;
		private readonly Promise promise;

		public PromiseAggregator(IEnumerable<IPromise> promises)
		{
			promise = new Promise();
			tasks = promises.Select(p =>
			{
				var cs = new TaskCompletionSource<Boolean>();
				p.Done(() => cs.SetResult(true)).Catch(promise.SignalException);
				return cs.Task;
			}).ToArray();
		}

		public Task Aggregate()
		{
			return Task.WhenAll(tasks);
		}
	}
}
