// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Threading.Tasks;

namespace A2v10.ProcS.Infrastructure
{
	public interface IRepository
	{
		Task<IInstance> Get(Guid id);
		Task Save(IInstance instance);

		Task<IInstance> CreateInstance(IIdentity identity);
		Task<IInstance> CreateInstance(IIdentity identity, Guid parentId);

		Task<IWorkflowDefinition> WorkflowFromStorage(IIdentity identity);
	}
}
