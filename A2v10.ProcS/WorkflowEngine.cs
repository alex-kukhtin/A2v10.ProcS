using A2v10.ProcS.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

/*
 */

namespace A2v10.ProcS
{
	public class WorkflowEngine
	{		
		public WorkflowEngine()
		{
		}

		public void Run(StateMachine stateMachine)
		{
			var instance = new WorkflowInstance()
			{
				Id = Guid.NewGuid()
			};
			var context = new ExecuteContext(instance);
			stateMachine.Run(context);
		}
	}
}
