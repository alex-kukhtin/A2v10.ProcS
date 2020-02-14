// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Threading.Tasks;
using A2v10.ProcS.Infrastructure;

namespace A2v10.ProcS
{
	public class Transition
	{
		public String Condition { get; set; }
		public String Description { get; set; }

		public String To { get; set; }
		public IWorkflowAction Action { get; set; }

		public Boolean Evaluate(IExecuteContext context)
		{
			if (String.IsNullOrEmpty(Condition))
				return true; // no condition, always true
			return context.EvaluateScript<Boolean>(Condition);
		}

		public async Task ExecuteAction(IExecuteContext context)
		{
			if (Action != null)
				await Action.Execute(context);
		}
	}
}
