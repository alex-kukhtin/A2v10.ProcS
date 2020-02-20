// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

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
		public IInstance Instance;
		public String WorkflowState;
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

		public Task<IInstance> Load(Guid instanceId)
		{
			if (_instances.TryGetValue(instanceId, out InstanceItem item))
			{
				//var workflow = await WorkflowFromStorage(instance.Workflow.GetIdentity());
				//workflow.SetState()
				//instance.Workflow = workflow;
				if (item.WorkflowState != null)
				{
					var workflowState = DynamicObject.FromJson(item.WorkflowState);
					item.Instance.Workflow.Restore(workflowState);
				}

				return Task.FromResult(item.Instance);
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

			if (_instances.ContainsKey(instance.Id))
				_instances[instance.Id].WorkflowState = stateJson;
			else
				_instances.Add(instance.Id, new InstanceItem()
				{
					Instance = instance,
					WorkflowState = stateJson
				});
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
