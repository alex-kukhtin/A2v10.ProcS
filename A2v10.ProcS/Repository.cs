// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using A2v10.ProcS.Infrastructure;
using Microsoft.Extensions.Configuration;

namespace A2v10.ProcS
{
	public class Repository : IRepository
	{
		private readonly IWorkflowStorage _workflowStorage;
		private readonly IInstanceStorage _instanceStorage;
		private readonly IConfiguration _config;

		private readonly Dictionary<Guid, IInstance> _instanceCache = new Dictionary<Guid, IInstance>();

		public Repository(IWorkflowStorage workflowStorage, IInstanceStorage instanceStorage, IConfiguration config)
		{
			_workflowStorage = workflowStorage ?? throw new ArgumentNullException(nameof(workflowStorage));
			_instanceStorage = instanceStorage ?? throw new ArgumentNullException(nameof(instanceStorage));
			_config = config;
		}

		async public Task<IInstance> Get(Guid id)
		{
			if (_instanceCache.TryGetValue(id, out IInstance instance))
				return instance;
			instance = await _instanceStorage.Load(id);
			SetInstanceEnvironment(instance);
			_instanceCache.Add(id, instance);
			return instance;
		}

		void SetInstanceEnvironment(IInstance instance)
		{
			if (_config == null)
				return;
			// TODO: cache????
			var env = _config.GetSection("ProcS:Environment").AsDynamic();
			if (env != null)
				instance.SetEnvironment(env);

		}
		public Task Save(IInstance instance)
		{
			return _instanceStorage.Save(instance);
		}

		public Task<IWorkflowDefinition> WorkflowFromStorage(IIdentity identity)
		{
			return _workflowStorage.WorkflowFromStorage(identity);
		}

		public async Task<IInstance> CreateInstance(IIdentity identity)
		{
			var workflow = await WorkflowFromStorage(identity);
			var inst = new Instance()
			{
				Id = Guid.NewGuid(),
				Workflow = workflow
			};
			SetInstanceEnvironment(inst);
			return inst;
		}

		public async Task<IInstance> CreateInstance(IIdentity identity, Guid parentId)
		{
			var workflow = await WorkflowFromStorage(identity);
			var inst = new Instance()
			{
				Id = Guid.NewGuid(),
				ParentInstanceId = parentId,
				Workflow = workflow
			};
			SetInstanceEnvironment(inst);
			return inst;
		}
	}
}
