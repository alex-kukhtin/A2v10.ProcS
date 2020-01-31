using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace A2v10.ProcS
{
	public enum ActionResult
	{
		Success,
		Idle
	}

	public class WorkflowAction
	{
		public virtual  Task<ActionResult> Execute(ExecuteContext context)
		{
			return Task.FromResult(ActionResult.Success);
		}
	}
}
