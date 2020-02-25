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
		private readonly IResourceWrapper _resourceWrapper;
		private const String Schema = "[A2v10.ProcS]";

		public SqlServerSagaKeeper(ISagaResolver sagaResolver, IDbContext dbContext, IResourceWrapper resourceWrapper)
		{
			_sagaResolver = sagaResolver ?? throw new ArgumentNullException(nameof(sagaResolver));
			_dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
			_resourceWrapper = resourceWrapper ?? throw new ArgumentNullException(nameof(resourceWrapper));
		}

		public async Task<PickedSaga> PickSaga()
		{
			await foreach (var eo in _dbContext.ReadExpandoAsync(null, $"{Schema}.[Message.Peek]", null))
			{
				var dobj = new DynamicObject(eo);
				var msgjson = dobj.Get<String>("Message");
				var msgdo = DynamicObjectConverters.FromJson(msgjson);
				var message = _resourceWrapper.Unwrap<IMessage>(msgdo);

				var saga = GetSagaForMessage(message, out ISagaKeeperKey key, out Boolean isNew);
				if (saga == null)
				{

				}
				return new PickedSaga(key, saga, new ServiceBusItem(message));
			}
			return new PickedSaga(false);
		}

		private ISaga GetSagaForMessage(IMessage message, out ISagaKeeperKey key, out Boolean isNew)
		{
			var sagaFactory = _sagaResolver.GetSagaFactory(message.GetType());
			key = new SagaKeeperKey(sagaFactory.SagaKind, message.CorrelationId);
			isNew = false;
			/*
			var state = sagas.GetOrAdd(key, k =>
			{
				var saga = sagaFactory.CreateSaga();
				return new SagaState(saga);
			});
			if (Interlocked.CompareExchange(ref state.HoldLevel, 1, 0) == 0)
				return state.Saga;
			*/
			return null;
		}

		public Task ReleaseSaga(PickedSaga picked)
		{
			if (!picked.Available) 
				throw new InvalidOperationException("Saga is not picked");
			return SagaUpdate(picked.Saga, picked.Key);
		}

		public async Task SendMessage(IServiceBusItem item)
		{
			var msg = _resourceWrapper.Wrap(item.Message).Store();
			var json = DynamicObjectConverters.ToJson(msg);
			var prm = new DynamicObject();
			prm.Set("Message", json);
			await _dbContext.ExecuteExpandoAsync(null, $"{Schema}.[Message.Send]", prm);
			if (item.After != null)
				foreach (var a in item.After)
				{
					json = DynamicObjectConverters.ToJson(_resourceWrapper.Wrap(a).Store());
					prm.Set("Message", json);
					await _dbContext.ExecuteExpandoAsync(null, $"{Schema}.[Message.Send]", prm);
				}
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
			return _dbContext.ExecuteExpandoAsync(null, $"{Schema}.[Saga.Remove]", prms);
		}

		Task AddOrUpdate(ISagaKeeperKey key, SagaState state)
		{
			return Task.FromResult(0);
		}
	}
}
