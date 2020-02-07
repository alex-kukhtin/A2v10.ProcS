
using System;
using System.Threading.Tasks;
using A2v10.ProcS.Infrastructure;

namespace A2v10.ProcS.Plugin
{
	public class TaskPluginActionMessage : MessageBase<CorrelationId<Int32>>
	{
		public Guid Id { get; }
		public TaskPluginActionMessage(Guid instanceId, CorrelationId<Int32> correlationId) : base(correlationId)
		{
			Id = instanceId;
		}
	}


	class TestPluginActionSaga : SagaBaseDispatched<CorrelationId<Int32>, TaskPluginActionMessage>
	{
		public TestPluginActionSaga() : base(nameof(TestPluginActionSaga))
		{
		}

		public TestPluginActionSaga(String kind) : base(nameof(TestPluginActionSaga))
		{
		}

		protected override Task Handle(IHandleContext context, TaskPluginActionMessage message)
		{
			Int32 result = message.CorrelationId.Value.Value;
			var reply = $"{{result:{result}}}";
			context.ResumeProcess(message.Id, reply);
			return Task.CompletedTask;
		}
	}

	class TestPluginSagaRegistrar : ISagaRegistrar
	{
		public void Register(ISagaManager mgr, IServiceProvider provider)
		{
			var factory = new ConstructSagaFactory<TestPluginActionSaga>(nameof(TestPluginActionSaga));
			mgr.RegisterSagaFactory<TaskPluginActionMessage>(factory);
		}
	}
}
