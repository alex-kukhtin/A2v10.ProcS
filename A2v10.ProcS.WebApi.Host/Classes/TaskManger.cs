// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using A2v10.ProcS.Infrastructure;

namespace A2v10.ProcS.WebApi.Host.Classes
{
	public class TaskManager : ITaskManager
	{
		private readonly List<(Task, Promise)> promises;

		public TaskManager()
		{
			promises = new List<(Task, Promise)>();
		}

		public IPromise AddTask(Func<Task> task)
		{
			var p = new Promise();
			
			var t = Task.Run(task).ContinueWith(t =>
			{
				if (t.IsCompleted) p.SignalDone();
				if (t.IsFaulted) p.SignalEception(t.Exception);
				if (t.IsCanceled) p.SignalEception(new TaskCanceledException(t));
			});
			promises.Add((t, p));
			return p;
		}
	}
}
