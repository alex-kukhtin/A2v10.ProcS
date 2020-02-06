// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace A2v10.ProcS.Infrastructure
{
	public interface IWorkflowDefinition
	{
		Task Run(IExecuteContext context);
		Task Resume(IResumeContext context);

		IIdentity GetIdentity();
		void SetIdentity(IIdentity identity);
	}
}
