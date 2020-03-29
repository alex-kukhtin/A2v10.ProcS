// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using A2v10.ProcS.Infrastructure;

namespace A2v10.ProcS
{
	[ResourceKey(ProcS.ResName + ":" + nameof(DelayActivity))]
	public class DelayActivity : IActivity
	{
		public TimeSpan Duration { get; set; }

		public ActivityExecutionResult Execute(IExecuteContext context)
		{
			if (context.IsContinue)
				return ActivityExecutionResult.Complete;
			var m = new ContinueActivityMessage(context.Instance.Id, String.Empty);
			context.SendMessageAfter(DateTime.UtcNow.Add(Duration), m);
			return ActivityExecutionResult.Idle;
		}
	}
}
