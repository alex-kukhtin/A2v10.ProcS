// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using A2v10.ProcS.Infrastructure;
using Newtonsoft.Json.Serialization;

namespace A2v10.ProcS.WebApi.Host.Classes
{
	public class FakeStorage : IWorkflowStorage, IInstanceStorage
	{
		private readonly Dictionary<Guid, IInstance> _instances = new Dictionary<Guid, IInstance>();

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
			if (_instances.TryGetValue(instanceId, out IInstance instance))
			{
				var workflow = await WorkflowFromStorage(instance.Workflow.GetIdentity());
				instance.Workflow = workflow;
				return instance;
			}
			throw new ArgumentOutOfRangeException(instanceId.ToString());
		}

		public Task Save(IInstance instance)
		{
			if (_instances.ContainsKey(instance.Id))
				_instances[instance.Id] = instance;
			else
				_instances.Add(instance.Id, instance);
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
				TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple
			});
			result.SetIdentity(identity);
			return Task.FromResult<IWorkflowDefinition>(result);
		}
		#endregion
	}

	public class ActualContractResolver : DefaultContractResolver
	{
		public override JsonContract ResolveContract(Type type)
		{
			if (type == typeof(IWorkflowAction))
			{
				return base.ResolveContract(typeof(CodeAction));
			}
			return base.ResolveContract(type);
		}
	}
}
