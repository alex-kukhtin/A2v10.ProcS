// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Collections.Generic;
using System.Dynamic;
using A2v10.ProcS.Infrastructure;

namespace A2v10.ProcS
{
	public enum ContinueActivityCondition
	{
		All,
		Any
	}

	public class ParallelActivity : IActivity, IStorable
	{
		public List<IActivity> Activities { get; set; }

		public ContinueActivityCondition ContinueWhen { get; set; }

		Int32 _waiting = 0;

		public ActivityExecutionResult Execute(IExecuteContext context)
		{
			if (context.IsContinue)
			{
				if (ContinueWhen == ContinueActivityCondition.Any)
					return ActivityExecutionResult.Complete;

				_waiting -= 1;
				if (_waiting == 0)
					return ActivityExecutionResult.Complete;
				return ActivityExecutionResult.Idle;
			}

			if (Activities == null || Activities.Count == 0)
				return ActivityExecutionResult.Complete;
			foreach (var activity in Activities)
			{
				var result = activity.Execute(context);
				if (result == ActivityExecutionResult.Idle)
					_waiting += 1;
			}
			return (_waiting > 0) ? ActivityExecutionResult.Idle : ActivityExecutionResult.Complete;
		}

		#region IStorable

		const string waitingName = "waiting";
		public IDynamicObject Store()
		{
			dynamic dd = new ExpandoObject();
			dd.waiting = _waiting;
			return new DynamicObject(dd);
		}
		public void Restore(IDynamicObject store)
		{
			dynamic dd = store.Root;
			_waiting = dd.waiting;
		}

		#endregion
	}
}
