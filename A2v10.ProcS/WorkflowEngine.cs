using A2v10.ProcS.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

/*
 */

namespace A2v10.ProcS
{
	public class WorkflowEngine
	{
		private readonly IWorkflowServiceBus _serviceBus;

		public WorkflowEngine(IWorkflowServiceBus bus)
		{
			_serviceBus = bus ?? throw new ArgumentNullException(nameof(bus));
		}

		public async Task Run(StateMachine stateMachine)
		{
			var instance = new WorkflowInstance()
			{
				Id = Guid.NewGuid()
			};
			var context = new ExecuteContext(_serviceBus, instance);
			await stateMachine.Run(context);
		}
	}
}
