// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;
using System.Threading.Tasks;
using A2v10.ProcS.Interfaces;

namespace A2v10.ProcS
{
	public class StartDomain : IMessage, IStartMessage
	{
		public Guid Id { get; }
		public String CorrelationId { get; }
	}

	public class ResumeProcess : IMessage
	{
		public String CorrelationId { get; }

		public Guid Id { get; }
		public String Result { get; }

		public ResumeProcess(Guid id, String result)
		{
			Id = id;
			Result = result;
		}
	}

	public class ProcessSaga : SagaBase
	{
		public static void Register()
		{
			A2v10.ProcS.ServiceBus.RegisterSaga<StartDomain, ProcessSaga>();
			A2v10.ProcS.ServiceBus.RegisterSaga<ResumeProcess, ProcessSaga>();
		}

		public ProcessSaga(Guid id, IServiceBus serviceBus, IInstanceStorage instanceStorage)
			: base(id, serviceBus, instanceStorage)
		{
		}

		#region dispatch
		public override Task<String> Handle(IMessage message)
		{
			switch (message)
			{
				case ResumeProcess resumeProcess:
					return HandleResume(resumeProcess);
			}
			return Task.FromResult<String>(null);
		}
		#endregion

		public async Task<String> HandleResume(ResumeProcess message)
		{
			var instance = await InstanceStorage.Load(message.Id);
			var context = new ResumeContext(ServiceBus, InstanceStorage, instance)
			{
				Result = message.Result
			};
			await instance.Workflow.Resume(context);
			return null;
		}
	}
}
