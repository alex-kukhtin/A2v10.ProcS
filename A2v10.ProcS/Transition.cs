
using System;
using System.Collections.Generic;
using System.Text;

using A2v10.ProcS.Interfaces;

namespace A2v10.ProcS
{
	public class Transition
	{
		public Boolean Default { get; set; }
		public String Description { get; set; }

		public String To { get; set; }
		public IWorkflowAction Action { get; set; }

		public Boolean Evaluate(IWorkflowExecuteContext context)
		{
			return true;
		}
	}
}
