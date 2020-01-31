
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using A2v10.ProcS.Interfaces;

namespace A2v10.ProcS
{
	public enum ExecuteResult
	{
		Continue,
		Idle,
		Exit
	}

	public class State
	{
		public String Description { get; set; }
		public Dictionary<String, Transition>  Transitions { get; set; }
		public Boolean Final { get; set; }

		public WorkflowAction OnEntry { get; set; }
		public WorkflowAction OnExit { get; set; }

		public ExecuteResult ExecuteStep(ExecuteContext context)
		{
			EnterState(context);
			var next = NextState(context);
			if (next == null)
				return ExecuteResult.Exit;
			next.Action?.Execute(context);
			ExitState(context);
			context.SetState(next.To);
			return ExecuteResult.Continue;
		}

		Transition NextState(ExecuteContext context)
		{
			if (Transitions == null || Transitions.Count == 0 || Final)
				return null;
			var next = Transitions.Where(kv => kv.Value.Evaluate(context)).Select(kv => kv.Value).FirstOrDefault();
			if (next == null)
				next = Transitions.Where(kv => kv.Value.Default).Select(kv => kv.Value).FirstOrDefault();
			return next;

		}

		void EnterState(ExecuteContext context)
		{
			OnEntry?.Execute(context);
		}

		void ExitState(ExecuteContext context)
		{
			OnExit?.Execute(context);
		}
	}
}
