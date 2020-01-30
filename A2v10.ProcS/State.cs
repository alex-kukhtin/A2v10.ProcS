
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using A2v10.ProcS.Interfaces;

namespace A2v10.ProcS
{
	public class State
	{
		public IWorkflowAction Entry { get; set; }
		public IWorkflowAction Exit { get; set; }

		public Dictionary<String, Transition>  Transitions { get; set; }

		public String ExecuteStep(ExecuteContext context)
		{
			EnterState(context);
			var next = Transitions.Where(kv => kv.Value.Evaluate(context)).Select(kv => kv.Value).FirstOrDefault();
			if (next == null)
				next = Transitions.Where(kv => kv.Value.Default).Select(kv => kv.Value).FirstOrDefault();
			if (next == null)
				return null;
			next.Action?.Execute();
			ExitState(context);
			return next.State;
		}

		public void ContinueStep(ExecuteContext context)
		{

		}

		void EnterState(ExecuteContext context)
		{
			Entry?.Execute();
		}

		void ExitState(ExecuteContext context)
		{
			Exit?.Execute();
		}
	}
}
