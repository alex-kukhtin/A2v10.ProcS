// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using A2v10.ProcS.Infrastructure;

namespace A2v10.ProcS
{
	public class Delay : IActivity
	{
		public TimeSpan Duration { get; set; }

		public ActivityExecutionResult Execute(IExecuteContext context)
		{
			//String bookmark = Guid.NewGuid().ToString();
			// TODO
			return ActivityExecutionResult.Idle;
		}
	}
}
