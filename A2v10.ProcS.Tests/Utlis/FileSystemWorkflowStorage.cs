using A2v10.ProcS.Infrastructure;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace A2v10.ProcS.Tests
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
