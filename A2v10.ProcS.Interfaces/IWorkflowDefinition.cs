using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace A2v10.ProcS.Interfaces
{
	public interface IWorkflowDefinition
	{
		Task Run(IWorkflowExecuteContext context);
	}
}
