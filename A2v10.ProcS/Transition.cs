
using System;
using System.Collections.Generic;
using System.Text;

using A2v10.ProcS.Interfaces;

namespace A2v10.ProcS
{
	public class Transition
	{
		public Boolean Default { get; set; }
		public String State { get; set; }
		public IWorkflowAction Action { get; set; }

		public Boolean Evaluate(ExecuteContext context)
		{
			return true;
		}
	}
}
