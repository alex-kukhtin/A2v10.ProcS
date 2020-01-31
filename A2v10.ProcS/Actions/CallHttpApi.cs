// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using A2v10.ProcS.Interfaces;

namespace A2v10.ProcS
{
	public class CallHttpApi : WorkflowAction
	{
		public String Url { get; set; }
		public String Method { get; set; }

		public override Task<ActionResult> Execute(ExecuteContext context)
		{
			//var url = context.Resolve(Url);
			//context.ScheduleAction("CallApi")
			//result = await GetWeather("");

			return Task.FromResult(ActionResult.Success);
		}
	}
}
