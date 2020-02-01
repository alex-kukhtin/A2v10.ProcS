using A2v10.ProcS.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace A2v10.ProcS
{
	public class ExecuteContext : IWorkflowExecuteContext
	{
		public IWorkflowInstance Instance { get; }

		private readonly IServiceBus _serviceBus;
		private readonly IInstanceStorage _instanceStorage;

		public ExecuteContext(IServiceBus bus, IInstanceStorage storage, WorkflowInstance instance)
		{
			_serviceBus = bus;
			_instanceStorage = storage;
			Instance = instance;

		}

		public async Task SaveInstance()
		{
			await _instanceStorage.Save(Instance);
		}

		public void ScheduleAction(String Bookmark, Object message)
		{
			// bookmark, instance, message
			_serviceBus.Send(message);
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
