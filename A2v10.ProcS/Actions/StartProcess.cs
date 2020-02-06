// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;
using System.Threading.Tasks;
using A2v10.ProcS.Infrastructure;

namespace A2v10.ProcS
{
	public class StartProcess : IWorkflowAction
	{
		public String Process { get; set; }

		public Task<ActionResult> Execute(IExecuteContext context)
		{
			throw new NotImplementedException();
		}
	}
}
