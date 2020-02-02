// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using A2v10.ProcS.Interfaces;

/*
 */

namespace A2v10.ProcS
{
	public class StartDomain : IMessage
	{
		public Guid Id { get; } = Guid.NewGuid();
	}

	public class ResumeProcess : IMessage, IDomainEvent
	{
		public Guid Id { get; private set; }

		public ResumeProcess(Guid id)
		{
			Id = id;
		}
	}

	public class DomainEventsSaga: SagaBase
	{

		public DomainEventsSaga(Guid id, IServiceBus serviceBus, IInstanceStorage instanceStorage)
			:base(id, serviceBus, instanceStorage)
		{ 
		}

		#region dispatch
		public override Task Handle(IMessage message)
		{
			switch (message)
			{
				case ResumeProcess resumeProcess:
					return HandleResume(resumeProcess);
			}
			return Task.FromResult(0);
		}
		#endregion

		public async Task HandleResume(ResumeProcess message)
		{
			var instance = await InstanceStorage.Load(message.Id);
			var context = new ExecuteContext(ServiceBus, InstanceStorage, instance);
			await instance.Workflow.Resume(context);
		}
	}

	public class WorkflowEngine : IWorkflowEngine
	{
		private readonly IServiceBus _serviceBus;
		private readonly IWorkflowStorage _workflowStorage;
		private readonly IInstanceStorage _instanceStorage;

		public WorkflowEngine(IWorkflowStorage workflowStorage, IInstanceStorage instanceStorage, IServiceBus serviceBus)
		{
			_serviceBus = serviceBus ?? throw new ArgumentNullException(nameof(serviceBus));
			_workflowStorage = workflowStorage ?? throw new ArgumentNullException(nameof(workflowStorage));
			_instanceStorage = instanceStorage ?? throw new ArgumentNullException(nameof(instanceStorage));
			serviceBus.Send(new StartDomain());
		}

		public static void RegisterSagas()
		{
			CallHttpApiSaga.Register();
			ServiceBus.RegisterSaga<StartDomain, DomainEventsSaga>();
		}

		#region IWorkflowEngine
		public async Task<IInstance> CreateInstance(IIdentity identity)
		{
			var workflow = await _workflowStorage.WorkflowFromStorage(identity);
			return CreateInstance(workflow);
		}

		public IInstance CreateInstance(IWorkflowDefinition workflow)
		{
			return new Instance()
			{
				Id = Guid.NewGuid(),
				Workflow = workflow
			};
		}


		public async Task<IInstance> StartWorkflow(IIdentity identity)
		{
			return await Run(identity);
		}

		public async Task<IInstance> ResumeWorkflow(Guid instaceId)
		{
			var instance = await _instanceStorage.Load(instaceId);
			var context = new ExecuteContext(_serviceBus, _instanceStorage, instance);
			await instance.Workflow.Resume(context);
			return instance;
		}
		#endregion


		public async Task<IInstance> Run(IIdentity identity)
		{
			var instance = await CreateInstance(identity);
			var context = new ExecuteContext(_serviceBus, _instanceStorage, instance);
			await instance.Workflow.Run(context);
			return instance;
		}

		public async Task<IInstance> Run(IWorkflowDefinition workflow)
		{
			var instance = CreateInstance(workflow);
			var context = new ExecuteContext(_serviceBus, _instanceStorage, instance);
			await instance.Workflow.Run(context);
			return instance;
		}
	}
}
