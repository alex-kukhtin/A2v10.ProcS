﻿// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Threading.Tasks;

using A2v10.ProcS.Infrastructure;

namespace A2v10.ProcS
{
	[ResourceKey(ProcS.ResName + ":" + nameof(WaitResumeActivity))]
	public class WaitResumeActivity : IActivity
	{
		public String Tag { get; set; }

		public ActivityExecutionResult Execute(IExecuteContext context)
		{
			if (context.IsContinue)
				return ActivityExecutionResult.Complete;

			var book = context.SetBookmark();

			var mess = new WaitBookmarkResumeMessage(book, context.Instance.Id, Tag);

			context.SendMessage(mess);

			return ActivityExecutionResult.Idle;
		}
	}
}
