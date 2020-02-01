
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using A2v10.ProcS.Interfaces;

namespace A2v10.ProcS
{
	public class Transition
	{
		public Boolean Default { get; set; }
		public String Description { get; set; }

		public String To { get; set; }
		public IWorkflowAction Action { get; set; }

		public Boolean Evaluate(IExecuteContext context)
		{
			return true;
		}

		public async Task ExecuteAction(IExecuteContext context)
		{
			if (Action != null)
				await Action.Execute(context);
		}
	}
}
