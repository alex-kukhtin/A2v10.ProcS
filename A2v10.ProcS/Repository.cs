
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using A2v10.ProcS.Interfaces;

namespace A2v10.ProcS
{
	public class Repository : IRepository
	{
		private readonly IWorkflowStorage _workflowStorage;
		private readonly IInstanceStorage _instanceStorage;

		public Repository(IWorkflowStorage workflowStorage, IInstanceStorage instanceStorage)
		{
			_workflowStorage = workflowStorage ?? throw new ArgumentNullException(nameof(workflowStorage));
			_instanceStorage = instanceStorage ?? throw new ArgumentNullException(nameof(instanceStorage));
		}

		public Task<IInstance> Get(Guid id)
		{
			return _instanceStorage.Load(id);
		}

		public Task Save(IInstance instance)
		{
			return _instanceStorage.Save(instance);
		}

		public Task<IWorkflowDefinition> WorkflowFromStorage(IIdentity identity)
		{
			return _workflowStorage.WorkflowFromStorage(identity);
		}
	}
}
