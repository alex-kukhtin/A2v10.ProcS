// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Threading.Tasks;

using A2v10.ProcS.Infrastructure;

namespace A2v10.ProcS
{
	public class WaitApiCallback : IActivity
	{
		public String Tag { get; set; }

		public String CorrelationValue { get; set; }
		public String CorrelationExpression { get; set; }

		public ActivityExecutionResult Execute(IExecuteContext context)
		{
			if (context.IsContinue)
				return ActivityExecutionResult.Complete;

			var mess = new WaitCallbackMessage(Tag) {
				CorrelationExpression = CorrelationExpression
			};
			context.SendMessage(mess);

			var mess2 = new WaitCallbackMessageProcess(context.Instance.Id, Tag, CorrelationValue) { 
				CorrelationExpression = CorrelationExpression
			};
			context.SendMessage(mess2);
			return ActivityExecutionResult.Idle;
		}
	}
}
