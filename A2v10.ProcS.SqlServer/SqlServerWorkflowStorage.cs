// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.IO;
using System.Threading.Tasks;
using A2v10.Data.Interfaces;
using A2v10.ProcS.Infrastructure;

namespace A2v10.ProcS.SqlServer
{
	public class SqlServerWorkflowStorage : WorkflowStorageBase
	{
		private readonly IDbContext _dbContext;
		private readonly IWorkflowCatalogue _catalogue;

		public SqlServerWorkflowStorage(IDbContext dbContext, IWorkflowCatalogue catalogue, IResourceWrapper resourceWrapper) : base(resourceWrapper)
		{
			_dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
			_catalogue = catalogue ?? throw new ArgumentNullException(nameof(catalogue));
		}

		private class WorkflowData
		{
			public String Id { get; set; }
			public Int32 Version { get; set; }
			public Guid Hash { get; set; }
			public String Body { get; set; }
		}

		public override async Task<IWorkflowDefinition> WorkflowFromStorage(IIdentity identity)
		{
			Identity nid;
			String json;
			if (identity.Version == 0)
			{
				json = await _catalogue.WorkflowFromCatalogue(identity.ProcessId);
				var h = GetJsonHash(json);
				var dd = await _dbContext.LoadAsync<WorkflowData>(null, "A2v10_ProcS.[Workflows.Update]", new
				{
					Id = identity.ProcessId,
					Hash = h,
					Body = json
				});
				nid = new Identity(dd.Id, dd.Version);
			}
			else
			{
				var dd = await _dbContext.LoadAsync<WorkflowData>(null, "A2v10_ProcS.[Workflows.Load]", new
				{
					Id = identity.ProcessId,
					Version = identity.Version
				});
				nid = new Identity(dd.Id, dd.Version);
				json = dd.Body;
			}
			var result = WorkflowFromJson(json);
			result.SetIdentity(nid);
			return result;
		}
	}
}
