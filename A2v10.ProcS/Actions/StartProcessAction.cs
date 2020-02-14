// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Dynamic;
using System.Threading.Tasks;
using A2v10.ProcS.Infrastructure;

namespace A2v10.ProcS
{
	public class StartProcess : IWorkflowAction
	{
		public String Process { get; set; }

		public String ParameterExpression { get; set; } // params <=

		public String SetResult { get; set; } //  reply => 

		public async Task<ActionResult> Execute(IExecuteContext context)
		{
			await context.SaveInstance();
			var prms = context.EvaluateScript<ExpandoObject>(ParameterExpression);
			var startMessage = new StartProcessMessage(context.Instance.Id)
			{				
				ProcessId = Process,
				Parameters = new DynamicObject(prms)
			};
			context.SendMessage(startMessage);
			return ActionResult.Idle;
		}
	}
}
