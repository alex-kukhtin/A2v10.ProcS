// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;
using System.Threading.Tasks;

namespace A2v10.ProcS.Infrastructure
{
	public interface IWorkflowEngine
	{
		Task<IInstance> StartWorkflow(IIdentity identity, IDynamicObject prms = null);
		Task<IInstance> ResumeWorkflow(Guid instaceId, String bookmark, String result);

		Task<IInstance> CreateInstance(IIdentity identity);
		IInstance CreateInstance(IWorkflowDefinition workflow);

		Task RunServiceBus();
	}
}
