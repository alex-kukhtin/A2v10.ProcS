// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using A2v10.ProcS.Infrastructure;

namespace A2v10.ProcS
{
	public enum ExecuteResult
	{
		Continue,
		Idle,
		Exit,
		Complete
	}

	public class State
	{
		public String Description { get; set; }
		public Dictionary<String, Transition>  Transitions { get; set; }


		public IWorkflowAction OnEntry { get; set; }
		public IWorkflowAction OnExit { get; set; }

		public String NextState { get; set; }

		public async Task<ExecuteResult> ExecuteStep(IExecuteContext context)
		{
			if (await EnterState(context) == ActionResult.Idle)
			{
				//context.SaveInstance();
				var resume = new InitResumeSagaMessage(context.ResumeId);
				context.SendMessage(resume);
				return ExecuteResult.Idle;
			}
			return await DoContinue(context);
		}

		public async Task ContinueStep(IResumeContext context)
		{
			context.ScriptContext.SetValue("result", context.Result);
			
			await DoContinue(context);
		}

		async Task<ExecuteResult> DoContinue(IExecuteContext context)
		{
			var next = TransitionToNextState(context);
			String nextState = NextState;
			if (next != null)
			{
				nextState = next.To;
				await next.ExecuteAction(context);
			}
			await ExitState(context);
			if (!String.IsNullOrEmpty(nextState))
			{
				context.Instance.SetState(nextState);
				return ExecuteResult.Continue;
			}
			context.ProcessComplete();
			return ExecuteResult.Complete;
		}

		Transition TransitionToNextState(IExecuteContext context)
		{
			if (Transitions == null || Transitions.Count == 0)
				return null;
			return Transitions.Where(kv => kv.Value.Evaluate(context)).Select(kv => kv.Value).FirstOrDefault();

		}

		async Task<ActionResult> EnterState(IExecuteContext context)
		{
			if (OnEntry == null)
				return ActionResult.Success;
			return await OnEntry.Execute(context);
		}

		async Task ExitState(IExecuteContext context)
		{
			if (OnExit == null)
				return;
			await OnExit.Execute(context);
		}
	}
}
