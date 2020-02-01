// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;
using System.Threading.Tasks;

namespace A2v10.ProcS.Interfaces
{
	public interface IWorkflowEngine
	{
		Task StartWorkflow(String processId);
	}
}
