// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using A2v10.ProcS.Interfaces;

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
		public async Task<IInstance> CreateInstance(IIdentity identity)
		{
			var workflow = await _repository.WorkflowFromStorage(identity);
			return CreateInstance(workflow);
		}

		public IInstance CreateInstance(IWorkflowDefinition workflow)
		{
			return new Instance()
			{
				Id = Guid.NewGuid(),
				Workflow = workflow
			};
		}


		public async Task<IInstance> StartWorkflow(IIdentity identity, IDynamicObject data = null)
		{
			return await Run(identity, data);
		}

		public async Task<IInstance> ResumeWorkflow(Guid instaceId, String bookmark, String result)
		{
			var instance = await _repository.Get(instaceId);
			using (var scriptContext = _scriptEngine.CreateContext())
			{
				var context = new ResumeContext(_serviceBus, _repository, scriptContext, instance)
				{
					Bookmark = bookmark,
					Result = result
				};
				await instance.Workflow.Resume(context);
				return instance;
			}
		}
		#endregion


		public async Task<IInstance> Run(IIdentity identity, IDynamicObject data = null)
		{
			var instance = await CreateInstance(identity);
			if (data != null)
				instance.SetParameters(data);
			using (var scriptContext = _scriptEngine.CreateContext())
			{
				var context = new ExecuteContext(_serviceBus, _repository, scriptContext, instance);
				await instance.Workflow.Run(context);
				return instance;
			}
		}

		public async Task<IInstance> Run(IWorkflowDefinition workflow, IDynamicObject data = null)
		{
			var instance = CreateInstance(workflow);
			if (data != null)
				instance.SetParameters(data);
			using (var scriptContext = _scriptEngine.CreateContext())
			{
				var context = new ExecuteContext(_serviceBus, _repository, scriptContext, instance);
				await instance.Workflow.Run(context);
				return instance;
			}
		}

		public Task RunServiceBus()
		{
			return _serviceBus.Run();
		}
	}
}
