// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;

namespace A2v10.ProcS.Interfaces
{
	public interface IWorkflowStorage
	{
		IWorkflowDefinition WorkflowFromString(String source);
		IWorkflowDefinition WorkflowFromStorage(String processId, Int32 Version = -1);
	}
}
