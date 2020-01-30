using System;

namespace A2v10.ProcS.Interfaces
{
	public interface IInstanceStorage
	{
		IWorkflowInstance Create(String processId);
		IWorkflowInstance Load(String instanceId);
		void Save(IWorkflowInstance instance);
	}
}
