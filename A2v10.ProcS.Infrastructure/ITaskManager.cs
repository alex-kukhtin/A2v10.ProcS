// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System.Threading.Tasks;

namespace A2v10.ProcS.Infrastructure
{
	public interface ITaskManager
	{
		void AddTask(Task task);
	}
}
