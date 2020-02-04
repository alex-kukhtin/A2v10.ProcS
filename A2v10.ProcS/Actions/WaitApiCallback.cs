// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;
using System.Threading.Tasks;

using A2v10.ProcS.Interfaces;

namespace A2v10.ProcS
{
	public class WaitApiCallback : IWorkflowAction
	{
		public String Tag { get; set; }

		public String CorrelationValue { get; set; }
		public String CorrelationExpression { get; set; }

		async public Task<ActionResult> Execute(IExecuteContext context)
		{
			await context.SaveInstance();

			var mess = new WaitCallbackMessage(Tag) {
				CorrelationExpression = CorrelationExpression
			};
			context.SendMessage(mess);

			var mess2 = new WaitCallbackMessageProcess(context.Instance.Id, Tag, CorrelationValue) { 
				CorrelationExpression = CorrelationExpression
			};
			context.SendMessage(mess2);
			return ActionResult.Idle;
		}
	}
}
