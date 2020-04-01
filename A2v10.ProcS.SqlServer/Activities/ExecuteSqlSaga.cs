// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Threading.Tasks;
using A2v10.Data.Interfaces;
using A2v10.ProcS.Infrastructure;
using Microsoft.Extensions.Logging;

namespace A2v10.ProcS.SqlServer
{
	[ResourceKey(ukey)]
	public class ExecuteSqlMessage : MessageBase<Guid>
	{
		public const String ukey = SqlServerProcS.ResName + ":" + nameof(ExecuteSqlMessage);

		public String DataSource { get; set; }
		public String Procedure { get; set; }
		public IDynamicObject Parameters { get; set; }

		[RestoreWith]
		public ExecuteSqlMessage(Guid correlationId)
			: base(correlationId)
		{
		}

		public override void Store(IDynamicObject storage, IResourceWrapper _)
		{
			storage.Set(nameof(DataSource), DataSource);
			storage.Set(nameof(Procedure), Procedure);
			storage.Set(nameof(Parameters), Parameters);
		}

		public override void Restore(IDynamicObject store, IResourceWrapper wrapper)
		{
			DataSource = store.Get<String>(nameof(DataSource));
			Procedure = store.Get<String>(nameof(Procedure));
			Parameters = store.GetDynamicObject(nameof(Parameters));
		}
	}

	public class ExecuteSqlSaga : SagaBaseDispatched<Guid, ExecuteSqlMessage>
	{
		public const String ukey = SqlServerProcS.ResName + ":" + nameof(ExecuteSqlSaga);

		private readonly IDbContext _dbContext;

		public ExecuteSqlSaga(IDbContext dbContext) : base(ukey)
		{
			_dbContext = dbContext;
		}

		protected async override Task Handle(IHandleContext context, ExecuteSqlMessage message)
		{
			context.Logger.LogInformation($"ExecuteSqlSaga.Handle(ExecuteSqlMessage). Procedure ='{message.Procedure}'");
			await _dbContext.ExecuteExpandoAsync(message.DataSource, message.Procedure, message.Parameters.Root);
			IsComplete = true;
		}
	}
}
