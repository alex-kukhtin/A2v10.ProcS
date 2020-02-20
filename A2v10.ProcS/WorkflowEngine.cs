// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using System.Threading.Tasks;
using A2v10.ProcS.Infrastructure;

/*
 */

namespace A2v10.ProcS
{

	public class WorkflowEngine : IWorkflowEngine
	{
		private readonly IServiceBus _serviceBus;
		private readonly IRepository _repository;
		private readonly IScriptEngine _scriptEngine;

		public WorkflowEngine(IRepository repository, IServiceBus serviceBus, IScriptEngine scriptEngine)
		{
			_serviceBus = serviceBus ?? throw new ArgumentNullException(nameof(serviceBus));
			_repository = repository ?? throw new ArgumentNullException(nameof(repository));
			_scriptEngine = scriptEngine ?? throw new ArgumentNullException(nameof(scriptEngine));
		}

		#region IWorkflowEngine
		public async Task<IInstance> StartWorkflow(IIdentity identity, IDynamicObject data = null)
		{
			return await Run(identity, data);
		}

		public Task<IInstance> StartWorkflow(String processId, IDynamicObject prms = null)
		{
			var identity = new Identity(processId);
			return Run(identity, prms ?? null);
		}

		public async Task<IInstance> ResumeWorkflow(Guid instaceId, String bookmark, IDynamicObject result)
		{
			var instance = await _repository.Get(instaceId);
			using (var scriptContext = _scriptEngine.CreateContext())
			{
				var context = new ExecuteContext(_serviceBus, _repository, scriptContext, instance)
				{
					Bookmark = bookmark,
					Result = result
				};
				await instance.Workflow.Continue(context);
				return instance;
			}
		}

		public IDynamicObject CreateDynamicObject()
		{
			return new DynamicObject();
		}
		#endregion


		public async Task<IInstance> Run(IIdentity identity, IDynamicObject data = null)
		{
			var instance = await _repository.CreateInstance(identity);
			if (data != null)
				instance.SetParameters(data);
			using (var scriptContext = _scriptEngine.CreateContext())
			{
				var context = new ExecuteContext(_serviceBus, _repository, scriptContext, instance);
				await instance.Workflow.Run(context);
				return instance;
			}
		}
	}
}
