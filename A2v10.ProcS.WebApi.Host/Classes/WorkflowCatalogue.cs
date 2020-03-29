// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using A2v10.ProcS.Infrastructure;

namespace A2v10.ProcS.WebApi.Host.Classes
{
	public class FilesystemWorkflowCatalogue : IWorkflowCatalogue
	{
		private readonly String path;

		public FilesystemWorkflowCatalogue(String path)
		{
			this.path = path;
		}

		public Task<String> WorkflowFromCatalogue(String id)
		{
			return File.ReadAllTextAsync(Path.Combine(path, id) + ".json");
		}
	}
}
