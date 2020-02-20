// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Threading.Tasks;
using A2v10.ProcS.Infrastructure;

using System.Data.SqlClient;
using System.Data;

namespace A2v10.ProcS.SqlServer
{
	public class SqlServerInstanceStorage : IInstanceStorage
	{
		public SqlServerInstanceStorage()
		{
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

		public Task Save(IInstance instance)
		{
			throw new NotImplementedException();
		}
	}
}
