// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using A2v10.ProcS.Infrastructure;

namespace A2v10.ProcS.WebApi.Host.Classes
{
	public class TaskManager : ITaskManager
	{
		private readonly List<Task> tasks;

		public TaskManager()
		{
			tasks = new List<Task>();
		}

		public void AddTask(Func<Task> task)
		{
			var running = Task.Run(task);
			tasks.Add(running);
		}
	}
}
