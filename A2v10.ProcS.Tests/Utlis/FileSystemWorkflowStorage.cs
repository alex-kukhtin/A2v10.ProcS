using A2v10.ProcS.Infrastructure;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace A2v10.ProcS.Tests
{
	public class FileSystemWorkflowStorage : IWorkflowStorage
	{
		private readonly String path;

		public FileSystemWorkflowStorage() : this("../../../../Workflows/")
		{

		}

		public FileSystemWorkflowStorage(String path)
		{
			this.path = path;
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

		public IWorkflowDefinition WorkflowFromString(String source)
		{
			throw new NotImplementedException();
		}
	}
}
