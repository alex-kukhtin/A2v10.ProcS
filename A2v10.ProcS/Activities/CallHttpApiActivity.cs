// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Threading.Tasks;
using A2v10.ProcS.Infrastructure;

namespace A2v10.ProcS
{
	public class CallHttpApiActivity : IActivity
	{
		public String Url { get; set; }
		public String Method { get; set; }

		public ActivityExecutionResult Execute(IExecuteContext context)
		{
			if (context.IsContinue)
				return ActivityExecutionResult.Complete;
			var request = new CallApiRequestMessage()
			{
				Id = context.Instance.Id,
				Url = context.Resolve(Url),
				Method = context.Resolve(Method)
			};
			context.SendMessage(request);
			return ActivityExecutionResult.Idle;
		}
	}
}
