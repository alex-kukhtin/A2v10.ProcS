// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Threading.Tasks;
using A2v10.ProcS.Infrastructure;

using System.Data.SqlClient;
using System.Data;
using A2v10.Data.Interfaces;

namespace A2v10.ProcS.SqlServer
{
	public class SqlServerInstanceStorage : IInstanceStorage
	{
		private readonly IDbContext _dbContext;
		public SqlServerInstanceStorage(IDbContext dbContext)
		{
			_dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
		}

		public async Task<IInstance> Load(Guid instanceId)
		{
			String cnnString = "";
			using (var cnn = new SqlConnection(cnnString))
			{
				await cnn.OpenAsync();
				using (var cmd = cnn.CreateCommand())
				{
					cmd.CommandText = "";
					cmd.CommandType = CommandType.StoredProcedure;
					using (var rdr = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow))
					{
						if (await rdr.ReadAsync()) {
							// TODO: Name->FieldNo
							//var id = rdr.GetGuid(0);
							//var process = rdr.GetString(1);
							//var version = rdr.GetInt32(2);
							/*var inst = new Instance()
							{
								Id = id
							};
							*/
						}
					}
				}
			}
			throw new NotImplementedException("use InstanceFactory here");
		}

		public async Task Save(IInstance instance)
		{
			var identity = instance.Workflow.GetIdentity();
			var instanceState = instance.Store();
			var wfState = instance.Workflow.Store();
			DynamicObject di = new DynamicObject();
			di.Set("Id", instance.Id);
			di.Set("Parent", instance.ParentInstanceId);
			di.Set("Process", identity.ProcessId);
			di.Set("Version", identity.Version);
			di.Set("IsComplete", instance.IsComplete);
			di.Set("WorkflowState", wfState.ToJson());
			di.Set("InstanceState", instanceState.ToJson());
			await _dbContext.ExecuteExpandoAsync(null, "[A2v10.ProcS].[SaveInstance]", di.Root);
		}
	}
}
