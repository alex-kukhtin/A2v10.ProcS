
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

		public async Task<ExecuteResult> ExecuteStep(ExecuteContext context)
		{
			if (await EnterState(context) == ActionResult.Idle)
			{
				context.SaveInstance();
				return ExecuteResult.Idle;
			}
			return await DoContinue(context);
		}

		public async Task ContinueStep(ExecuteContext context)
		{
			await DoContinue(context);
		}

		async Task<ExecuteResult> DoContinue(ExecuteContext context)
		{
			var next = NextState(context);
			if (next == null)
				return ExecuteResult.Exit;
			next.Action?.Execute(context);
			await ExitState(context);
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

		async Task<ActionResult> EnterState(ExecuteContext context)
		{
			if (OnEntry == null)
				return ActionResult.Success;
			return await OnEntry.Execute(context);
		}

		async Task ExitState(ExecuteContext context)
		{
			if (OnExit == null)
				return;
			await OnExit?.Execute(context);
		}
	}
}
