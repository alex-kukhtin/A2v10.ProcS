// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.IO;
using Newtonsoft.Json;

using A2v10.ProcS.Infrastructure;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace A2v10.ProcS.Tests
{
	public class InstanceItem
	{
		public String WorkflowState;
		public String InstanceData;
		public IIdentity WorkflowIdentity;
	}

	public class FakeStorage : IWorkflowStorage, IInstanceStorage
	{
		private readonly Dictionary<Guid, InstanceItem> _instances = new Dictionary<Guid, InstanceItem>();

		private readonly String path;
		private readonly IResourceWrapper _wrapper;

		public FakeStorage(IResourceWrapper wrapper) : this(wrapper, "../../../../Workflows/")
		{
			
		}

		public FakeStorage(IResourceWrapper wrapper, String path)
		{
			_wrapper = wrapper;
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
					Workflow = await WorkflowFromStorage(item.WorkflowIdentity)
				};

				if (item.WorkflowState != null)
				{
					var workflowState = DynamicObjectConverters.FromJson(item.WorkflowState);
					instance.Workflow.Restore(workflowState, _wrapper);
				}

				instance.Restore(DynamicObjectConverters.FromJson(item.InstanceData), _wrapper);

				return instance;
			}
			throw new ArgumentOutOfRangeException(instanceId.ToString());
		}

		public Task Save(IInstance instance)
		{
			IDynamicObject state = instance.Workflow.Store(_wrapper);
			String stateJson = null;

			if (state != null)
			{
				stateJson = state.ToJson();
			}

			if (!_instances.TryGetValue(instance.Id, out InstanceItem item))
			{
				item = new InstanceItem()
				{
					WorkflowIdentity = instance.Workflow.GetIdentity()
				};
				_instances.Add(instance.Id, item);
			}
			item.WorkflowState = stateJson;
			item.InstanceData =  instance.Store(_wrapper).ToJson();
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
				ContractResolver = new ActivityContractResolver()
			}) as IWorkflowDefinition;
			result.SetIdentity(identity);
			return Task.FromResult(result);
		}
		#endregion
	}

	public class FakeLogger : ILogger
	{
		public IDisposable BeginScope<TState>(TState state)
		{
			throw new NotImplementedException();
		}

		public bool IsEnabled(LogLevel logLevel)
		{
			return false;
		}

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
		{

		}
	}
}
