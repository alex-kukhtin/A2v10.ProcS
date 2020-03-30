// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Threading.Tasks;

using A2v10.ProcS.Infrastructure;

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

			context.SendMessage(mess);

			return ActivityExecutionResult.Idle;
		}
	}
}
