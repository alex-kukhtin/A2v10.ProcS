using A2v10.ProcS.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace A2v10.ProcS
{
	public class ExecuteContext
	{
		public WorkflowInstance Instance { get; }

		public ExecuteContext(WorkflowInstance instance)
		{
			Instance = instance;
		}

		public void SetState(String state)
		{
			Instance.CurrentState = state;
		}

		public void ScheduleAction(String Bookmark, Task<DynamicObject> task)
		{
		}
	}

	public class ContinueContext
	{
		public IWorkflowInstance Instance { get; }
		public String Bookmark { get; }
	}
}
