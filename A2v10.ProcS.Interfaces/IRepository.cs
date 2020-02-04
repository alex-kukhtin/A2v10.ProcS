// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;
using System.Threading.Tasks;

namespace A2v10.ProcS.Interfaces
{
	public interface IRepository
	{
		Task<IInstance> Get(Guid id);
		Task Save(IInstance instance);

		Task<IWorkflowDefinition> WorkflowFromStorage(IIdentity identity);
	}
}
