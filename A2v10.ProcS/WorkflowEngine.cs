// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using A2v10.ProcS.Infrastructure;
using Microsoft.Extensions.Logging;

/*
 */

namespace A2v10.ProcS
{

	public class WorkflowEngine : IWorkflowEngine
	{
		private readonly IServiceBus _serviceBus;
		private readonly IRepository _repository;
		private readonly IScriptEngine _scriptEngine;
		private readonly ILogger _logger;
		private readonly INotifyManager _notifyManager;

		public WorkflowEngine(IRepository repository, IServiceBus serviceBus, IScriptEngine scriptEngine, ILogger logger, INotifyManager notifyManager)
		{
			_serviceBus = serviceBus ?? throw new ArgumentNullException(nameof(serviceBus));
			_repository = repository ?? throw new ArgumentNullException(nameof(repository));
			_scriptEngine = scriptEngine ?? throw new ArgumentNullException(nameof(scriptEngine));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_notifyManager = notifyManager ?? throw new ArgumentNullException(nameof(notifyManager));
		}

		#region IWorkflowEngine
		public async Task<IInstance> StartWorkflow(IIdentity identity, IDynamicObject prms = null)
		{
			return await Run(identity, prms);
		}

		public Task<IInstance> StartWorkflow(String processId, IDynamicObject prms = null)
		{
			return StartWorkflow(new Identity(processId), prms);
		}

		public Task ResumeBookmark(Guid instaceId, String tag, IDynamicObject result)
		{
			var m = new ResumeMessage(instaceId, tag);
			m.Result = result;
			_serviceBus.Send(m);
			return Task.CompletedTask;
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
				var context = new ExecuteContext(_serviceBus, _repository, scriptContext, _logger, _notifyManager, instance);
				await instance.Workflow.Run(context);
				return instance;
			}
		}
	}
}
