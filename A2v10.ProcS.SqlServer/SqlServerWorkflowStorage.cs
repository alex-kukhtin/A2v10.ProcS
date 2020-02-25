// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Threading.Tasks;
using A2v10.Data.Interfaces;
using A2v10.ProcS.Infrastructure;

namespace A2v10.ProcS.SqlServer
{
	public class SqlServerWorkflowStorage : IWorkflowStorage
	{
		private readonly IDbContext _dbContext;

		public SqlServerWorkflowStorage(IDbContext dbContext)
		{
			_dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
		}

		public Task<IWorkflowDefinition> WorkflowFromStorage(IIdentity identity)
		{
			throw new NotImplementedException();
		}

		public IWorkflowDefinition WorkflowFromString(String source)
		{
			throw new NotImplementedException();
		}
	}
}
