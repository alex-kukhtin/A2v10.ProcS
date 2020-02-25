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
			var eo = await _dbContext.ReadExpandoAsync(null, $"{Schema}.[Message.Peek]", null);
			if (eo == null)
				return new PickedSaga(false);

			var dobj = new DynamicObject(eo);

			var msgjson = dobj.Get<String>("Message");
			var msgdo = DynamicObjectConverters.FromJson(msgjson);
			var message = _resourceWrapper.Unwrap<IMessage>(msgdo);
				
			(ISaga saga, ISagaKeeperKey key) = await GetSagaForMessage(message);

			return new PickedSaga(key, saga, new ServiceBusItem(message));
		}

		private async Task<(ISaga saga, ISagaKeeperKey key)> GetSagaForMessage(IMessage message)
		{
			var sagaFactory = _sagaResolver.GetSagaFactory(message.GetType());
			var key = new SagaKeeperKey(sagaFactory.SagaKind, message.CorrelationId);
			var prms = new DynamicObject()
			{
				{ "Key", key.ToString() }
			};

			var eo = await _dbContext.ReadExpandoAsync(null, $"{Schema}.[Saga.GetOrAdd]", prms);

			var statejson = new DynamicObject(eo).Get<String>("State");
			var saga = sagaFactory.CreateSaga();
			saga.Restore(DynamicObjectConverters.FromJson(statejson));

			return (saga, key);
		}



			/*
			var state = sagas.GetOrAdd(key, k =>
			{
				var saga = sagaFactory.CreateSaga();
				return new SagaState(saga);
			});
			if (Interlocked.CompareExchange(ref state.HoldLevel, 1, 0) == 0)
				return state.Saga;
			*/

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
				await AddOrUpdate(key, ns);
			}
		}

		Task RemoveSaga(ISagaKeeperKey key)
		{
			var prms = new DynamicObject
			{
				{ "Key", key.ToString() }
			};
			return _dbContext.ExecuteExpandoAsync(null, $"{Schema}.[Saga.Remove]", prms);
		}

		Task AddOrUpdate(ISagaKeeperKey key, SagaState state)
		{
			var prms = new DynamicObject
			{
				{ "Key", key.ToString() },
				{ "State", DynamicObjectConverters.ToJson(state.Saga.Store()) },
				{ "Hold", state.HoldLevel }
			};
			return _dbContext.ExecuteExpandoAsync(null, $"{Schema}.[Saga.Update]", prms);
		}
	}
}
