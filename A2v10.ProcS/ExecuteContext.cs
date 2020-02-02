// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;
using System.Threading.Tasks;

using A2v10.ProcS.Interfaces;

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

		public String Resolve(String source)
		{
			return source;
		}
	}

	public class ResumeContext : ExecuteContext, IResumeContext
	{
		public String Bookmark { get; set; }
		public String Result { get; set; }

		public ResumeContext(IServiceBus bus, IInstanceStorage storage, IInstance instance)
			: base(bus, storage, instance)
		{
		}
	}
}
