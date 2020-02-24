// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Threading.Tasks;

using A2v10.ProcS.Infrastructure;

namespace A2v10.ProcS
{
	public class SyncTaskManager : ITaskManager
	{
		public IPromise AddTask(Func<Task> task)
		{
			var p = new Promise();
			try
			{
				Task.Run(task).Wait();
			}
			catch (Exception e)
			{
				p.SignalEception(e);
				return p;
			}
			p.SignalDone();
			return p;
		}
	}
}
