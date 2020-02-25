// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Threading.Tasks;

using A2v10.Data.Interfaces;
using A2v10.ProcS.Infrastructure;

namespace A2v10.ProcS.SqlServer
{
	public class SqlServerSagaKeeper : ISagaKeeper
	{
		private readonly ISagaResolver _sagaResolver;
		private readonly IDbContext _dbContext;
		private readonly IResourceManager _resourceManager;

		public SqlServerSagaKeeper(ISagaResolver sagaResolver, IDbContext dbContext, IResourceManager resourceManager)
		{
			_sagaResolver = sagaResolver ?? throw new ArgumentNullException(nameof(sagaResolver));
			_dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
			_resourceManager = resourceManager ?? throw new ArgumentNullException(nameof(resourceManager));
		}

		public Task<PickedSaga> PickSaga()
		{
			//foreach (var eo in _dbContext.LoadExpandoAsync(null, "[A2v10.ProcS].[GetMessage]");
			throw new NotImplementedException();
		}

		public Task ReleaseSaga(PickedSaga picked)
		{
			if (!picked.Available) 
				throw new InvalidOperationException("Saga is not picked");
			return SagaUpdate(picked.Saga, picked.Key);
		}

		public Task SendMessage(IServiceBusItem item)
		{
			var toStore = item.Message.Store();
			return _dbContext.ExecuteExpandoAsync(null, "[A2v10.ProcS].SendMessage", toStore.Root);
		}

		async Task SagaUpdate(ISaga saga, ISagaKeeperKey key)
		{
			if (saga.IsComplete || saga.CorrelationId == null || !saga.CorrelationId.Equals(key.CorrelationId))
			{
				await RemoveSaga(key);
			}
			if (!saga.IsComplete)
			{
				var ns = new SagaState(saga);
				await AddOrUpdate(new SagaKeeperKey(saga), ns);
			}
		}

		Task RemoveSaga(ISagaKeeperKey key)
		{
			DynamicObject prms = new DynamicObject();
			prms.Add("Kind", key.SagaKind);
			prms.Add("correlationId", key.CorrelationId.ToString());
			return _dbContext.ExecuteExpandoAsync(null, "[A2v10.ProcS].[RemoveSaga]", prms);
		}

		Task AddOrUpdate(ISagaKeeperKey key, SagaState state)
		{
			return Task.FromResult(0);
		}
	}
}
