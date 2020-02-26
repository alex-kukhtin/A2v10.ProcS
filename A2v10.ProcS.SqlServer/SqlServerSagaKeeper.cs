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
			Int64 msgid = dobj.Get<Int64>("Id");

			(Guid id, ISaga saga) = await GetSagaForMessage(message);

			return new PickedSaga(id, saga, new ServiceBusItem(message));
		}

		private async Task<(Guid id, ISaga saga)> GetSagaForMessage(IMessage message)
		{
			var sagaFactory = _sagaResolver.GetSagaFactory(message.GetType());
			var key = new SagaKeeperKey(sagaFactory.SagaKind, message.CorrelationId);
			var prms = new DynamicObject()
			{
				{ "Key", key.ToString() }
			};

			var eo = await _dbContext.ReadExpandoAsync(null, $"{Schema}.[Saga.GetOrAdd]", prms);
			var edo = new DynamicObject(eo);

			var saga = sagaFactory.CreateSaga();
			var statejson = edo.Get<String>("State");
			if (!String.IsNullOrEmpty(statejson))
				saga.Restore(DynamicObjectConverters.FromJson(statejson), _resourceWrapper);

			return (edo.Get<Guid>("Id"), saga);
		}

		public Task ReleaseSaga(PickedSaga picked)
		{
			if (!picked.Available) 
				throw new InvalidOperationException("Saga is not picked");
			return SagaUpdate(picked.Saga, picked.Id);
		}

		public async Task SendMessage(IServiceBusItem item)
		{
			var msg = _resourceWrapper.Wrap(item.Message).Store(_resourceWrapper);
			var json = DynamicObjectConverters.ToJson(msg);
			var prm = new DynamicObject();
			prm.Set("Message", json);
			prm.Set("Id", (Int64)0);
			await _dbContext.ExecuteExpandoAsync(null, $"{Schema}.[Message.Send]", prm);
			Int64 msgId = prm.Get<Int64>("Id");
			if (item.After != null)
			{
				foreach (var a in item.After)
				{
					var aftermsg = _resourceWrapper.Wrap(a.Message).Store(_resourceWrapper);
					var afterjson = DynamicObjectConverters.ToJson(aftermsg);
					prm.Set("Message", afterjson);
					prm.Set("Parent", msgId);
					await _dbContext.ExecuteExpandoAsync(null, $"{Schema}.[Message.Send]", prm);
				}
			}
		}

		Task SagaUpdate(ISaga saga, Guid id)
		{
			return Task.CompletedTask;
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
				{ "State", DynamicObjectConverters.ToJson(state.Saga.Store(_resourceWrapper)) },
				{ "Hold", state.HoldLevel }
			};
			return _dbContext.ExecuteExpandoAsync(null, $"{Schema}.[Saga.Update]", prms);
		}
	}
}
