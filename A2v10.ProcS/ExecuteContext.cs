using A2v10.ProcS.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace A2v10.ProcS
{
	public class ExecuteContext : IExecuteContext
	{
		public IInstance Instance { get; }

		private readonly IServiceBus _serviceBus;
		private readonly IInstanceStorage _instanceStorage;

		public ExecuteContext(IServiceBus bus, IInstanceStorage storage, IInstance instance)
		{
			_serviceBus = bus;
			_instanceStorage = storage;
			Instance = instance;
		}

		public async Task SaveInstance()
		{
			await _instanceStorage.Save(Instance);
		}

		public void SendMessage(IMessage message)
		{
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
