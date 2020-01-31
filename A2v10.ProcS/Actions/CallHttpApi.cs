// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using A2v10.ProcS.Interfaces;

namespace A2v10.ProcS
{
	public class CallHttpApi : WorkflowAction
	{
		public String Url { get; set; }
		public String Method { get; set; }

		public override void Execute(ExecuteContext context)
		{
			//context.ScheduleAction("CallApi")
		}
	}
}
