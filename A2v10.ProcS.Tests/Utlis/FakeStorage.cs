﻿// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.IO;
using Newtonsoft.Json;

using A2v10.ProcS.Infrastructure;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json.Serialization;

namespace A2v10.ProcS.Tests
{
	public class InstanceItem
	{
		public Boolean IsComplete;
		public String WorkflowState;
		public String InstanceData;
		public String InstanceResult;
		public String InstanceParams;
		public String CurrentState;
		public IIdentity WorkflowIdentity;
	}

	public class FakeStorage : IWorkflowStorage, IInstanceStorage
	{
		private readonly Dictionary<Guid, InstanceItem> _instances = new Dictionary<Guid, InstanceItem>();

		private readonly String path;

		public FakeStorage() : this("../../../../Workflows/")
		{

		}

		public FakeStorage(String path)
		{
			this.path = path;
		}

		#region IInstanceStorage
		public IInstance Create(Guid processId)
		{
			throw new NotImplementedException(nameof(Create));
		}

		public async Task<IInstance> Load(Guid instanceId)
		{
			if (_instances.TryGetValue(instanceId, out InstanceItem item))
			{
				Instance instance = new Instance()
				{
					Id = instanceId,
					Workflow = await WorkflowFromStorage(item.WorkflowIdentity),
					CurrentState = item.CurrentState,
					IsComplete = item.IsComplete
				};

				if (item.WorkflowState != null)
				{
					var workflowState = DynamicObject.FromJson(item.WorkflowState);
					instance.Workflow.Restore(workflowState);
				}
				if (item.InstanceData != null)
					instance.Data = DynamicObject.FromJson(item.InstanceData);
				if (item.InstanceResult != null)
					instance.Result = DynamicObject.FromJson(item.InstanceResult);
				if (item.InstanceParams != null)
					instance.Parameters = DynamicObject.FromJson(item.InstanceParams);

				return instance;
			}
			throw new ArgumentOutOfRangeException(instanceId.ToString());
		}

		public Task Save(IInstance instance)
		{
			IDynamicObject state = instance.Workflow.Store();
			String stateJson = null;

			if (state != null)
			{
				stateJson = state.ToJson();
			}

			InstanceItem item = null;
			if (!_instances.TryGetValue(instance.Id, out item)) {
				item = new InstanceItem()
				{
					WorkflowIdentity = instance.Workflow.GetIdentity()
				};
				_instances.Add(instance.Id, item);
			}
			item.IsComplete = instance.IsComplete;
			item.WorkflowState = stateJson;
			item.CurrentState = instance.CurrentState;
			item.InstanceData = instance.GetData().ToJson();
			item.InstanceResult = instance.GetResult().ToJson();
			item.InstanceParams = instance.GetParameters().ToJson();
			return Task.FromResult(0);
		}

		#endregion

		#region IWorkflowStorage
		public IWorkflowDefinition FromString(String source)
		{
			throw new NotImplementedException(nameof(FromString));
		}

		public IWorkflowDefinition WorkflowFromString(String source)
		{
			throw new NotImplementedException(nameof(WorkflowFromString));
		}

		public Task<IWorkflowDefinition> WorkflowFromStorage(IIdentity identity)
		{
			String json = File.ReadAllText(Path.Combine(path, identity.ProcessId));
			var result = JsonConvert.DeserializeObject<StateMachine>(json, new JsonSerializerSettings()
			{
				TypeNameHandling = TypeNameHandling.Auto,
				ContractResolver = new ActualContractResolver()
			}) as IWorkflowDefinition;
			result.SetIdentity(identity);
			return Task.FromResult(result);
		}
		#endregion
	}

	public class ActualContractResolver : DefaultContractResolver
	{
		public override JsonContract ResolveContract(Type type)
		{
			if (type == typeof(IActivity))
			{
				return base.ResolveContract(typeof(CodeActivity));
			}
			return base.ResolveContract(type);
		}
	}
}
