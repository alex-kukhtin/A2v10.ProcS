// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Threading.Tasks;

using A2v10.ProcS.Infrastructure;

namespace A2v10.ProcS
{
	[ResourceKey(ProcS.ResName + ":" + nameof(SendCallbackActivity))]
	public class SendCallbackActivity : IActivity
	{
		public String Tag { get; set; }
		public String DataExpression { get; set; }

		public ActivityExecutionResult Execute(IExecuteContext context)
		{
			var data = context.EvaluateScriptObject(DataExpression);

			var msg = new CallbackMessage(Tag)
			{
				Result = data
			};

			context.SendMessage(msg);

			return ActivityExecutionResult.Complete;
		}
	}
}
