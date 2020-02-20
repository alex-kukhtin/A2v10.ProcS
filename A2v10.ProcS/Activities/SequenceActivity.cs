// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Collections.Generic;
using System.Dynamic;
using A2v10.ProcS.Infrastructure;

namespace A2v10.ProcS
{
	public class SequenceActivity : IActivity, IStorable
	{
		public List<IActivity> Activities { get; set; }

		Int32 _currentAction { get; set; }

		public ActivityExecutionResult Execute(IExecuteContext context)
		{
			if (Activities == null || Activities.Count == 0)
				return ActivityExecutionResult.Complete;
			while (true)
			{
				if (_currentAction >= Activities.Count)
					return ActivityExecutionResult.Complete;
				var result = Activities[_currentAction].Execute(context);
				if (result == ActivityExecutionResult.Idle)
					return ActivityExecutionResult.Idle;
				_currentAction += 1;
			}
		}

		const String currentActionName = "Current";

		#region IStorable
		public IDynamicObject Store()
		{
			var list = new List<Object>();
			foreach (var activity in Activities)
			{
				if (activity is IStorable storable)
					list.Add(storable.Store().Root);
				else
					list.Add(null);
			}
			var ret = new DynamicObject();
			ret.Set(nameof(Activities), list);
			ret.Set(currentActionName, _currentAction);
			return ret;
		}

		public void Restore(IDynamicObject store)
		{
			_currentAction = store.Get<Int32>(currentActionName);
			var activities = store.Get<List<Object>>(nameof(Activities));
			for (int i=0; i<activities.Count; i++)
			{
				var elem = DynamicObject.From(activities[i]);
				if (elem != null && Activities[i] is IStorable storable)
				{
					storable.Restore(elem);
				}
			}
		}
		#endregion
	}
}
