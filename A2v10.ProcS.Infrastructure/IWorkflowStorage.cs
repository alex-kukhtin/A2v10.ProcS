// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.IO;
using System.Threading.Tasks;

namespace A2v10.ProcS.Infrastructure
{
	public interface IWorkflowStorage
	{
		Task<IWorkflowDefinition> WorkflowFromStorage(IIdentity identity);
	}

	public interface IWorkflowCatalogue
	{
		Task<String> WorkflowFromCatalogue(String id);
	}
}
