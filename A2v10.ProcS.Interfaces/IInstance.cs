// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;

namespace A2v10.ProcS.Interfaces
{
	public interface IInstance
	{
		Guid Id { get; }
		IWorkflowDefinition Workflow { get; set; }

		String CurrentState { get; set; }
		void SetState(String state);
	}
}
