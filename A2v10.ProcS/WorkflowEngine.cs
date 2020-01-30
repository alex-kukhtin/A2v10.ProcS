using A2v10.ProcS.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

/*
 */

namespace A2v10.ProcS
{
	public class WorkflowEngine : IWorkflowEngine
	{
		private readonly IInstanceStorage _storage;
		
		public WorkflowEngine(IInstanceStorage storage)
		{
			_storage = storage ?? throw new ArgumentNullException(nameof(storage));
		}

		public IWorkflowInstance Create(String definitionId)
		{
			return _storage.Create(definitionId);
		}

		public void Execute(String instanceId)
		{
			IWorkflowInstance instance = _storage.Load(instanceId);
		}
	}
}
