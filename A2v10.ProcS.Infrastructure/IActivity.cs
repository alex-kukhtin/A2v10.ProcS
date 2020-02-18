// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Threading.Tasks;

namespace A2v10.ProcS.Infrastructure
{
	public enum ActivityExecutionResult
	{
		Idle,
		Complete
	}

	public interface IActivity
	{
		ActivityExecutionResult Execute(IExecuteContext context);
	}
}
