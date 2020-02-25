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
		private const String Schema = "[A2v10.ProcS]";

		public SqlServerSagaKeeper(ISagaResolver sagaResolver, IDbContext dbContext, IResourceManager resourceManager)
		{
			_sagaResolver = sagaResolver ?? throw new ArgumentNullException(nameof(sagaResolver));
			_dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
			_resourceManager = resourceManager ?? throw new ArgumentNullException(nameof(resourceManager));
		}

		public Task<PickedSaga> PickSaga()
		{
			//foreach (var eo in _dbContext.LoadExpandoAsync(null, "[A2v10.ProcS].[GetMessage]");
			/*
			await foreach (var x in _dbContext.ReadExpandoAsync(null, $"{Schema}.[Saga.Peek]", null) {
				var dobj = new DynamicObject(x);
				var msgjson = dobj.Get<String>("Message");
				var msgdo = DynamicObjectConverters.FromJson(msgjson);
				var msg = _resourceManager.Unwrap<IMessage>(msgdo);
			}
			//throw new NotImplementedException();
			*/
			return Task.FromResult<PickedSaga>(new PickedSaga());
		}

		public Task ReleaseSaga(PickedSaga picked)
		{
			if (!picked.Available) 
				throw new InvalidOperationException("Saga is not picked");
			return SagaUpdate(picked.Saga, picked.Key);
		}

		public async Task SendMessage(IServiceBusItem item)
		{
			var toStore = item.Message.Store();
			await _dbContext.ExecuteExpandoAsync(null, $"{Schema}.[Message.Send]", item.Message.Store().Root);
			if (item.After != null)
				foreach (var a in item.After)
					await _dbContext.ExecuteExpandoAsync(null, $"{Schema}.[Message.Send]", a.Message.Store().Root);
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
