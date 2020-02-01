// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;
using System.Threading.Tasks;

namespace A2v10.ProcS.Interfaces
{
	public enum ActionResult
	{
		Success,
		Fail,
		Idle
	}

	public interface IWorkflowAction
	{
		Task<ActionResult> Execute(IWorkflowExecuteContext context);
	}
}
