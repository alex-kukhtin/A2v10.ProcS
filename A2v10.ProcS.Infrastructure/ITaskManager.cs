// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Threading.Tasks;

namespace A2v10.ProcS.Infrastructure
{
	public interface IPromise
	{
		IPromise Done(Action action);
		IPromise Catch(Action<Exception> action);
	}

	public interface IPromise<T>
	{
		IPromise<T> Done(Action<T> action);
		IPromise<T> Catch(Action<Exception> action);
	}

	public interface ITaskManager
	{
		IPromise AddTask(Func<Task> task);
	}

	public interface INotifyManager
	{
		void Register(Guid id, Promise<String> promise);
		Promise<String> GetAndRemove(Guid id);
	}
}
