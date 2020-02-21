// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using A2v10.ProcS.Infrastructure;

namespace A2v10.ProcS
{
	public enum ContinueActivityCondition
	{
		All,
		Any
	}

	[ResourceKey(ProcS.ResName + ":" + nameof(ParallelActivity))]
	public class ParallelActivity : IActivity, IStorable
	{
		public List<IActivity> Activities { get; set; }

		public ContinueActivityCondition ContinueWhen { get; set; }

		List<Boolean> _waiting = new List<Boolean>();

		public ActivityExecutionResult Execute(IExecuteContext context)
		{
			if (context.IsContinue)
			{
				for (var i=0; i<_waiting.Count; i++)
				{
					if (_waiting[i])
					{
						var result = Activities[i].Execute(context);
						if (result == ActivityExecutionResult.Complete)
							_waiting[i] = false;
					}
				}
				switch (ContinueWhen) {
					case ContinueActivityCondition.Any:
						return _waiting.Any(x => !x) ? ActivityExecutionResult.Complete : ActivityExecutionResult.Idle;
					case ContinueActivityCondition.All:
						return _waiting.All(x => !x) ? ActivityExecutionResult.Complete : ActivityExecutionResult.Idle;
				}
			}

			if (Activities == null || Activities.Count == 0)
				return ActivityExecutionResult.Complete;

			foreach (var activity in Activities)
			{
				var result = activity.Execute(context);
				if (result == ActivityExecutionResult.Idle)
					_waiting.Add(true);
				else
					_waiting.Add(false);
			}
			return _waiting.Any(x => x) ? ActivityExecutionResult.Idle : ActivityExecutionResult.Complete;
		}

		#region IStorable

		const String waitingName = "Waiting";

		public IDynamicObject Store()
		{
			var d = new DynamicObject();
			d.Set(waitingName, _waiting);
			return d;
		}

		public void Restore(IDynamicObject store)
		{
			var elems = store.GetListOrNull<Boolean>(waitingName);
			if (elems != null)
				_waiting = elems;
			else
				_waiting.Clear();
		}

		#endregion
	}
}
