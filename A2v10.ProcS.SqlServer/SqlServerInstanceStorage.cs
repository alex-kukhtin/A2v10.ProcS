
using System;
using System.Threading.Tasks;
using A2v10.ProcS.Interfaces;

using System.Data.SqlClient;
using System.Data;

namespace A2v10.ProcS.SqlServer
{
	public class SqlServerInstanceStorage : IInstanceStorage
	{
		public IInstance Create(Guid processId)
		{
			throw new NotImplementedException();
		}

		public async Task<IInstance> Load(Guid instanceId)
		{
			String cnnString = "";
			using (var cnn = new SqlConnection(cnnString))
			{
				await cnn.OpenAsync();
				using (var cmd = cnn.CreateCommand())
				{
					using (var rdr = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow))
					{
						if (await rdr.ReadAsync()) {

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
