// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Threading.Tasks;

using A2v10.ProcS.Infrastructure;

namespace A2v10.ProcS
{
	public class CodeAction : IWorkflowAction
	{
		public String Code { get; set; }

		public Task<ActionResult> Execute(IExecuteContext context)
		{
			context.ExecuteScript(Code);
			return Task.FromResult(ActionResult.Success);
		}
	}
}
