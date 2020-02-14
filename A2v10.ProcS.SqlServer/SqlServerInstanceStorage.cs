// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Threading.Tasks;
using A2v10.ProcS.Infrastructure;

using System.Data.SqlClient;
using System.Data;

namespace A2v10.ProcS.SqlServer
{
	public interface IInstanceFactory
	{
		IInstance CreateInstance(Guid id, IIdentity identity);
	}

	public class SqlServerInstanceStorage : IInstanceStorage
	{
		private readonly IInstanceFactory _factory;

		public SqlServerInstanceStorage(IInstanceFactory factory)
		{
			_factory = factory ?? throw new ArgumentNullException(nameof(factory));
		}

		public IInstance Create(Guid id, IIdentity identity)
		{
			return _factory.CreateInstance(id, identity);
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
