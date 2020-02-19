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


		public IActivity OnEntry { get; set; }
		public IActivity OnExit { get; set; }

		public String NextState { get; set; }

		public ActivityExecutionResult Execute(IExecuteContext context)
		{
			ActivityExecutionResult result;
			String nextState = NextState;
			// on entry
			if (OnEntry != null)
			{
				result = OnEntry.Execute(context);
				if (result == ActivityExecutionResult.Idle)
					return result;
				context.IsContinue = false;
			}
			// transitions
			var nextTransition = TransitionToNextState(context);
			if (nextTransition != null)
			{
				nextState = nextTransition.To;
				result = nextTransition.Execute(context);
				if (result == ActivityExecutionResult.Idle)
					return result;
				context.IsContinue = false;
			}
			if (OnExit != null)
			{
				result = OnExit.Execute(context);
				if (result == ActivityExecutionResult.Idle)
					return result;
				context.IsContinue = false;
			}
			context.Instance.SetState(nextState);
			return ActivityExecutionResult.Complete;
		}

		Transition TransitionToNextState(IExecuteContext context)
		{
			if (Transitions == null || Transitions.Count == 0)
				return null;
			return Transitions.Where(kv => kv.Value.Evaluate(context)).Select(kv => kv.Value).FirstOrDefault();
		}

		 Task<ActivityExecutionResult> EnterState(IExecuteContext context)
		{
			if (OnEntry == null)
				return Task.FromResult(ActivityExecutionResult.Complete);
			OnEntry.Execute(context);
			return Task.FromResult(ActivityExecutionResult.Complete);
		}

		Task ExitState(IExecuteContext context)
		{
			if (OnExit == null)
				return Task.CompletedTask;
			OnExit.Execute(context);
			return Task.CompletedTask;
		}
	}
}
