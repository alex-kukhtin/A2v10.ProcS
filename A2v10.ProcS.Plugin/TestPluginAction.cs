// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Threading.Tasks;
using A2v10.ProcS.Infrastructure;

[assembly: ProcSPlugin]

namespace A2v10.ProcS.Plugin
{
	public class TestPluginAction : IWorkflowAction
	{
		public Int32 CorrelationId { get; set; }

		public Task<ActionResult> Execute(IExecuteContext context)
		{
			var corrId = new CorrelationId<Int32>(42);
			context.SaveInstance();
			context.SendMessage(new TaskPluginActionMessage(context.Instance.Id, corrId));

			return Task.FromResult(ActionResult.Idle);
		}
	}
}
