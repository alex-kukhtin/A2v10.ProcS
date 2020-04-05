// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Dynamic;
using System.Threading.Tasks;
using A2v10.ProcS.Infrastructure;
using Newtonsoft.Json;

namespace A2v10.ProcS
{
	public enum ErrorMode
	{
		Transition,
		Throw,
		Ignore,
	}

	[ResourceKey(ProcS.ResName + ":" + nameof(CallHttpApiActivity))]
	public class CallHttpApiActivity : IActivity
	{
		public String Url { get; set; }
		public String Method { get; set; }

		public String Body { get; set; }

		public String CodeBefore { get; set; }
		public String CodeAfter { get; set; }

		public ErrorMode HandleError { get; set; }

		public ActivityExecutionResult Execute(IExecuteContext context)
		{
			if (context.IsContinue)
			{
				context.ExecuteScript(CodeAfter);
				return ActivityExecutionResult.Complete;
			}

			context.ExecuteScript(CodeBefore);

			var bookmark = context.SetBookmark();

			var request = new CallApiRequestMessage(bookmark)
			{
				Url = context.Resolve(Url),
				Method = context.Resolve(Method),
				HandleError = HandleError
			};

			var body = context.EvaluateScript(Body);
			if (body != null)
			{
				if (body is ExpandoObject bodyEO)
					request.Body = JsonConvert.SerializeObject(bodyEO, new DoubleConverter());
				else
					request.Body = body.ToString();
			}
			context.SendMessage(request);
			return ActivityExecutionResult.Idle;
		}
	}
}
