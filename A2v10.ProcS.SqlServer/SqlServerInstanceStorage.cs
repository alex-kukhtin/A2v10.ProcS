// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using A2v10.Data.Interfaces;
using A2v10.ProcS.Infrastructure;

namespace A2v10.ProcS.SqlServer
{
	public class SqlServerInstanceStorage : IInstanceStorage
	{
		private readonly IDbContext _dbContext;
		private readonly IWorkflowStorage _workflowStorage;
		private readonly IResourceWrapper _resourceWrapper;
		private readonly ILogger _logger;


		private const String Schema = "[A2v10_ProcS]";

		public SqlServerInstanceStorage(IWorkflowStorage workflowStorage, IDbContext dbContext, IResourceWrapper resourceWrapper, ILogger logger)
		{
			_workflowStorage = workflowStorage ?? throw new ArgumentNullException(nameof(workflowStorage));
			_dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
			_resourceWrapper = resourceWrapper ?? throw new ArgumentNullException(nameof(resourceWrapper));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public async Task<IInstance> Load(Guid instanceId)
		{
			var prms = new DynamicObject();
			prms.Set("Id", instanceId);
			var eo = await _dbContext.ReadExpandoAsync(null, $"{Schema}.[Instance.Load]", prms);
			if (eo == null)
				throw new ArgumentOutOfRangeException($"Instance '{instanceId}' not found");
			var di = new DynamicObject(eo);
			var identity = new Identity(di.Get<String>("Workflow"), di.Get<Int32>("Version"));
			var inst = new Instance()
			{
				Id = instanceId,
				Workflow = await _workflowStorage.WorkflowFromStorage(identity)
			};
			var instanceState = DynamicObjectConverters.FromJson(di.Get<String>("InstanceState"));
			var workflowState = DynamicObjectConverters.FromJson(di.Get<String>("WorkflowState"));
			_logger.LogInformation($"Instance.Load. Id:'{instanceId}'");
			inst.Restore(instanceState, _resourceWrapper);
			inst.Workflow.Restore(workflowState, _resourceWrapper);
			return inst;
		}

		public async Task Save(IInstance instance)
		{
			var identity = instance.Workflow.GetIdentity();
			var instanceState = instance.Store(_resourceWrapper);
			var wfState = instance.Workflow.Store(_resourceWrapper);

			DynamicObject di = new DynamicObject();

			di.Set("Id", instance.Id);
			if (instance.ParentInstanceId != Guid.Empty)
				di.Set("Parent", instance.ParentInstanceId);
			di.Set("Workflow", identity.ProcessId);
			di.Set("Version", identity.Version);
			di.Set("IsComplete", instance.IsComplete);
			di.Set("WorkflowState", wfState.ToJson());
			di.Set("InstanceState", instanceState.ToJson());
			di.Set("CurrentState", instance.CurrentState);

			_logger.LogInformation($"Instance.Save. Id:'{instance.Id}'");
			await _dbContext.ExecuteExpandoAsync(null, $"{Schema}.[Instance.Save]", di.Root);
		}
	}
}
