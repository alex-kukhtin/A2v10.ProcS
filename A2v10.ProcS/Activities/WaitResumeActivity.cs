// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;

using A2v10.ProcS.Infrastructure;
using Microsoft.Extensions.Logging;

namespace A2v10.ProcS
{
	[ResourceKey(ProcS.ResName + ":" + nameof(WaitResumeActivity))]
	public class WaitResumeActivity : IActivity
	{
		public String Bookmark { get; set; }

		public ActivityExecutionResult Execute(IExecuteContext context)
		{
			if (context.IsContinue)
				return ActivityExecutionResult.Complete;

			var book = context.SetBookmark();

			var mess = new WaitResumeMessage(book, context.Instance.Id, Bookmark);

			context.Logger.LogInformation($"WaitResumeActivity.Execute. Send 'WaitResumeMessage'. Bookmark='{Bookmark}'");
			context.SendMessage(mess);

			return ActivityExecutionResult.Idle;
		}
	}
}
