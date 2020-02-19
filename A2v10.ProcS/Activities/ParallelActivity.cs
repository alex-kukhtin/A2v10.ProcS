// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Collections.Generic;

using A2v10.ProcS.Infrastructure;

namespace A2v10.ProcS
{

	public class ParallelActivity : IActivity, IStorable
	{
		public List<IActivity> Activities { get; set; }

		public ActivityExecutionResult Execute(IExecuteContext context)
		{
			if (Activities == null || Activities.Count == 0)
				return ActivityExecutionResult.Complete;
			foreach (var activity in Activities)
			{
				//if (!activity.IsComplete) ????
				var result = activity.Execute(context);
				if (result == ActivityExecutionResult.Idle)
				{
					// save here???
				}
			}
			// ?????
			return ActivityExecutionResult.Idle;
		}

		#region IStorable
		public IDynamicObject Store()
		{
			throw new NotImplementedException();
		}

		public void Restore(IDynamicObject store)
		{
			throw new NotImplementedException();
		}
		#endregion
	}
}
