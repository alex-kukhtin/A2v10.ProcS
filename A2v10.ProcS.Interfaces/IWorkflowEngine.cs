
using System;
using System.Collections.Generic;
using System.Text;

namespace A2v10.ProcS.Interfaces
{
	public interface IWorkflowEngine
	{
		IWorkflowInstance Create(String processId);

		void Execute(String instanceId);
	}
}
