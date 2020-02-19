// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Dynamic;
using System.Threading.Tasks;
using A2v10.ProcS.Infrastructure;

namespace A2v10.ProcS
{
	public class StartProcessActivity : IActivity
	{
		public String Process { get; set; }
		public String ParameterExpression { get; set; } // params <=

		CorrelationId<Guid> CorrelationId = new CorrelationId<Guid>(Guid.NewGuid());

		public ActivityExecutionResult Execute(IExecuteContext context)
		{
			if (context.IsContinue)
				return ActivityExecutionResult.Complete;

			var prms = context.EvaluateScript<ExpandoObject>(ParameterExpression);
			var startMessage = new StartProcessMessage(context.Instance.Id)
			{
				ProcessId = Process,
				Parameters = new DynamicObject(prms)
			};
			context.SendMessage(startMessage);
			return ActivityExecutionResult.Idle;
		}
	}
}
