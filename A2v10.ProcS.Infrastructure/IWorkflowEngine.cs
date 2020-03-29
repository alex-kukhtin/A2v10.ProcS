// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Threading.Tasks;

namespace A2v10.ProcS.Infrastructure
{
	public interface IWorkflowEngine
	{
		Task<IInstance> StartWorkflow(IIdentity identity, IDynamicObject prms = null);
		Task<IInstance> StartWorkflow(String processId, IDynamicObject prms = null);

		Task ResumeBookmark(Guid instaceId, String tag, IDynamicObject result);

		IDynamicObject CreateDynamicObject();

		//Task<IInstance> CreateInstance(IIdentity identity);
	}
}
