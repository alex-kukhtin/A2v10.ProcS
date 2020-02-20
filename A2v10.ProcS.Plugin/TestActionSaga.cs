// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Dynamic;
using System.Threading.Tasks;
using A2v10.ProcS.Infrastructure;

namespace A2v10.ProcS.Plugin
{
	[ResourceKey(ukey)]
	public class TaskPluginActionMessage : MessageBase<CorrelationId<Int32>>
	{
		public const string ukey = "com.a2.procs.test:" + nameof(TaskPluginActionMessage);
		public Guid Id { get; }
		public TaskPluginActionMessage(Guid instanceId, CorrelationId<Int32> correlationId) : base(correlationId)
		{
			Id = instanceId;
		}
	}


	class TestPluginActionSaga : SagaBaseDispatched<CorrelationId<Int32>, TaskPluginActionMessage>
	{
		public const string ukey = "com.a2.procs.test:" + nameof(TestPluginActionSaga);

		public TestPluginActionSaga() : base(ukey)
		{
		}

		public TestPluginActionSaga(String kind) : base(ukey)
		{
		}

		protected override Task Handle(IHandleContext context, TaskPluginActionMessage message)
		{
			Int32 result = message.CorrelationId.Value.Value;
			var reply = $"{{result:{result}}}";
			context.ContinueProcess(message.Id, null, reply);
			return Task.CompletedTask;
		}
	}

	class TestPluginSagaRegistrar : ISagaRegistrar
	{
		public void Register(IResourceManager rmgr, ISagaManager smgr)
		{
			var factory = new ConstructSagaFactory<TestPluginActionSaga>(nameof(TestPluginActionSaga));
			rmgr.RegisterResourceFactory(factory.SagaKind, new SagaResourceFactory(factory));
			rmgr.RegisterResources(TestPluginActionSaga.GetHandledTypes());
			smgr.RegisterSagaFactory(factory, TestPluginActionSaga.GetHandledTypes());
		}
	}
}
