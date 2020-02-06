// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;
using System.Threading.Tasks;
using A2v10.ProcS.Infrastructure;

namespace A2v10.ProcS
{
	public class CallHttpApi : IWorkflowAction
	{
		public String Url { get; set; }
		public String Method { get; set; }
		public String ResultExpression { get; set; }

		async public Task<ActionResult> Execute(IExecuteContext context)
		{
			await context.SaveInstance();
			var request = new CallApiRequestMessage()
			{
				Id = context.Instance.Id,
				Url = context.Resolve(Url),
				Method = context.Resolve(Method)
			};
			context.SendMessage(request);
			return ActionResult.Idle;
		}
	}
}
