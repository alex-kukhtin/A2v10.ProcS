// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Threading.Tasks;
using A2v10.ProcS.Infrastructure;

[assembly: ProcSPlugin]

namespace A2v10.ProcS.Plugin
{
	public class TestPluginActivity : IActivity
	{
		public Int32 CorrelationId { get; set; }

		public ActivityExecutionResult Execute(IExecuteContext context)
		{
			if (context.IsContinue)
				return ActivityExecutionResult.Complete;

			var corrId = new CorrelationId<Int32>(42);
			context.SaveInstance();
			context.SendMessage(new TaskPluginActionMessage(context.Instance.Id, corrId));

			return ActivityExecutionResult.Idle;
		}
	}
}
