using System;
using System.IO;
using System.Threading.Tasks;

using Newtonsoft.Json;

using A2v10.ProcS.Infrastructure;

namespace A2v10.ProcS.Tests.SqlStorage
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
				ContractResolver = new ActivityContractResolver()
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
