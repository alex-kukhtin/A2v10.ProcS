// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;
using System.Threading.Tasks;

namespace A2v10.ProcS.Interfaces
{
	public interface IInstanceStorage
	{
		IWorkflowInstance Create(Guid processId);
		IWorkflowInstance Load(Guid instanceId);
		Task Save(IWorkflowInstance instance);
	}
}
