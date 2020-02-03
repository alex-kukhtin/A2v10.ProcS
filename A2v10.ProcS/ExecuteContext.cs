// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;
using System.Threading.Tasks;

using A2v10.ProcS.Interfaces;

namespace A2v10.ProcS
{
	public class HandleContext : IHandleContext
	{
		protected readonly IServiceBus _serviceBus;
		protected readonly IInstanceStorage _instanceStorage;
		protected readonly IScriptEngine _scriptEngine;

		public HandleContext(IServiceBus bus, IInstanceStorage storage)
		{
			_serviceBus = bus;
			_instanceStorage = storage;
			_scriptEngine = new ScriptEngine();
		}

		public Task<IInstance> LoadInstance(Guid id)
		{
			return _instanceStorage.Load(id);
		}

		public void SendMessage(IMessage message)
		{
			_serviceBus.Send(message);
		}

		public IResumeContext CreateResumeContext(IInstance instance)
		{
			return new ResumeContext(_serviceBus, _instanceStorage, instance);
		}

	}

	public class ExecuteContext : HandleContext, IExecuteContext
	{
		public IInstance Instance { get; }

		public ExecuteContext(IServiceBus bus, IInstanceStorage storage, IInstance instance)
			: base(bus, storage)
		{
			Instance = instance;
		}

		public async Task SaveInstance()
		{
			await _instanceStorage.Save(Instance);
		}

		public String Resolve(String source)
		{
			return source;
		}

		public T Evaluate<T>(String expression)
		{
			return _scriptEngine.Eval<T>(expression);
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
