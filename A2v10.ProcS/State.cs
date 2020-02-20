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

	public class State : IStorable
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


		#region IStorable
		public IDynamicObject Store()
		{
			var ret = new DynamicObject();
			if (OnEntry is IStorable storable)
			{
				var data = storable.Store();
				if (!data.IsEmpty)
					ret.Set(nameof(OnEntry), data);
			}
			if (OnExit is IStorable storable1)
			{
				var data = storable1.Store();
				if (!data.IsEmpty)
					ret.Set(nameof(OnExit), data);
			}
			return ret;
		}

		public void Restore(IDynamicObject store)
		{
			var entry = store.GetDynamicObject(nameof(OnEntry));
			if (entry != null && OnEntry is IStorable storable)
				storable.Restore(entry);

			var exit = store.GetDynamicObject(nameof(OnExit));
			if (exit != null && OnExit is IStorable storable1)
				storable1.Restore(exit);
		}
		#endregion
	}
}
