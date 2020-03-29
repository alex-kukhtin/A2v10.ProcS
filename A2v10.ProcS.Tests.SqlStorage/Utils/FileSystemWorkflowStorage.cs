using System;
using System.IO;
using System.Threading.Tasks;

using Newtonsoft.Json;

using A2v10.ProcS.Infrastructure;

namespace A2v10.ProcS.Tests.SqlStorage
{
	public class FileSystemWorkflowStorage : WorkflowStorageBase, IWorkflowStorage
	{
		private readonly String path;

		public FileSystemWorkflowStorage(IResourceWrapper wrapper) : this(wrapper, "../../../../Workflows/")
		{

		}

		public FileSystemWorkflowStorage(IResourceWrapper wrapper, String path) : base(wrapper)
		{
			this.path = path;
		}

		public override Task<IWorkflowDefinition> WorkflowFromStorage(IIdentity identity)
		{
			String json = File.ReadAllText(Path.Combine(path, identity.ProcessId));
			var result = WorkflowFromJson(json);
			result.SetIdentity(identity);
			return Task.FromResult(result);
		}

		public IWorkflowDefinition WorkflowFromString(String source)
		{
			throw new NotImplementedException();
		}
	}
}
