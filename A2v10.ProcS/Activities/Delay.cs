// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using A2v10.ProcS.Infrastructure;

namespace A2v10.ProcS
{
	public class Delay : IActivity
	{
		public String Duration { get; set; }

		public ActivityExecutionResult Execute(IExecuteContext context)
		{
			TimeSpan span = TimeSpan.Parse(Duration);
			//String bookmark = Guid.NewGuid().ToString();
			// TODO
			return ActivityExecutionResult.Idle;
		}
	}
}
