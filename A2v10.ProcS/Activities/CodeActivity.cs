// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;

using A2v10.ProcS.Infrastructure;

namespace A2v10.ProcS
{
	public class CodeActivity : IActivity
	{
		public String Code { get; set; }

		public ActivityExecutionResult Execute(IExecuteContext context)
		{
			context.ExecuteScript(Code);
			return ActivityExecutionResult.Complete;
		}
	}
}
