// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System.Threading.Tasks;

using A2v10.ProcS.Infrastructure;

namespace A2v10.ProcS
{
	public class SyncTaskManager : ITaskManager
	{
		public void AddTask(Task task)
		{
			task.RunSynchronously();
			// Although the task was run synchronously, it is a good practice
			// to wait for it in the event exceptions were thrown by the task.
			task.Wait();
		}
	}
}
