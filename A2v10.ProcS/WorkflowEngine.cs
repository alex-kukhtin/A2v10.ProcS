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

			// register sagas
			CallHttpApiSaga.Register();
		}

		public async Task StartWorkflow(String processId)
		{
			var workflow = _workflowStorage.WorkflowFromStorage(processId, -1);
			var instance = new WorkflowInstance()
			{
				Id = Guid.NewGuid()
			};
			var context = new ExecuteContext(_serviceBus, _instanceStorage, instance);
			await workflow.Run(context);
		}

		public async Task Run(IWorkflowDefinition workflow)
		{
			var instance = new WorkflowInstance()
			{
				Id = Guid.NewGuid()
			};
			var context = new ExecuteContext(_serviceBus, _instanceStorage, instance);
			await workflow.Run(context);
		}
	}
}
