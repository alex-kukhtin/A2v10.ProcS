using System;
using System.Collections.Generic;
using System.Text;

namespace A2v10.ProcS.Interfaces
{
	public interface IWorkflowStorage
	{
		IWorkflowDefinition FromString(String source);
		IWorkflowDefinition FromStorage(Guid Id, Int32 Version);

		void Write(IWorkflowDefinition workflowDefinition);
	}
}
