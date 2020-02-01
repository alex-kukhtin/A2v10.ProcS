// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;
using System.IO;
using Newtonsoft.Json;

using A2v10.ProcS.Interfaces;
using System.Threading.Tasks;

namespace A2v10.ProcS.Tests
{
	public class FakeStorage : IWorkflowStorage, IInstanceStorage
	{
		#region IInstanceStorage
		public IWorkflowInstance Create(Guid processId)
		{
			throw new NotImplementedException(nameof(Create));
		}

		public IWorkflowInstance Load(Guid instanceId)
		{
			throw new NotImplementedException(nameof(Load));
		}

		public Task Save(IWorkflowInstance instance)
		{
			throw new NotImplementedException(nameof(Save));
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

		public IWorkflowDefinition WorkflowFromStorage(String processId, Int32 Version = -1)
		{
			String json = File.ReadAllText($"..//..//..//Workflows//{processId}");
			return JsonConvert.DeserializeObject<StateMachine>(json, new JsonSerializerSettings()
			{
				TypeNameHandling = TypeNameHandling.Auto
			});
		}
		#endregion
	}
}
