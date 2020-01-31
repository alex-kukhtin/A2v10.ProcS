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
		private IWorkflowServiceBus ServiceBus { get; }

		public ExecuteContext(IWorkflowServiceBus bus, WorkflowInstance instance)
		{
			Instance = instance;
			ServiceBus = bus;
		}

		public void SetState(String state)
		{
			Instance.CurrentState = state;
		}

		public void SaveInstance()
		{

		}

		public void ScheduleAction(String Bookmark, IServiceMessage message)
		{
			// bookmark, instance, message
			ServiceBus.Send(message);
		}
	}

	public class ContinueContext
	{
		public String Bookmark { get; }

		/*
		public ContinueContext(WorkflowInstance instance, String bookmark)
		{
			Bookmark = bookmark;
		}
		*/
	}
}
