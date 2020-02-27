// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Linq;
using System.Threading.Tasks;

using A2v10.Data.Interfaces;
using A2v10.ProcS.Infrastructure;

namespace A2v10.ProcS.SqlServer
{
	public class SagaMapItem
	{
		public SagaMapItem(String messageKind, String sagaKind)
		{
			MessageKind = messageKind;
			SagaKind = sagaKind;
		}

		public String MessageKind { get; }
		public String SagaKind { get; }
	}

	public class SqlServerSagaKeeper : ISagaKeeper
	{
		private readonly ISagaResolver _sagaResolver;
		private readonly IDbContext _dbContext;
		private readonly IResourceWrapper _resourceWrapper;

		// TODO: ??????????? Config????
		private static readonly Guid _host = Guid.Parse("BBDFE351-D6A1-4F22-9341-6FBD6628424B");

		private Boolean _sagaMapSaved;


		private const String Schema = "[A2v10_ProcS]";

		public SqlServerSagaKeeper(ISagaResolver sagaResolver, IDbContext dbContext, IResourceWrapper resourceWrapper)
		{
			_sagaResolver = sagaResolver ?? throw new ArgumentNullException(nameof(sagaResolver));
			_dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
			_resourceWrapper = resourceWrapper ?? throw new ArgumentNullException(nameof(resourceWrapper));
		}

		public async Task<PickedSaga> PickSaga()
		{
			await SaveSagaMap();

			var prms = new DynamicObject();
			prms.Set("Host", _host);
			var eo = await _dbContext.ReadExpandoAsync(null, $"{Schema}.[Message.Peek]", prms);

			if (eo == null)
				return new PickedSaga(false);

			var dobj = new DynamicObject(eo);

			return GetSagaFromMessage(dobj);
		}

		PickedSaga GetSagaFromMessage(DynamicObject dobj)
		{
			var msgjson = dobj.Get<String>("MessageBody");
			var msgdo = DynamicObjectConverters.FromJson(msgjson);
			var message = _resourceWrapper.Unwrap<IMessage>(msgdo);

			Guid? sagaId = dobj.Get<Guid?>("SagaId");
			
			var sagakind = dobj.Get<String>("SagaKind");
			var sagabodyjson = dobj.Get<String>("SagaBody");

			ISaga saga;
			if (!String.IsNullOrEmpty(sagabodyjson))
			{
				var sagbodydo = DynamicObjectConverters.FromJson(sagabodyjson);
				var sagaRes = new Resource(sagakind, sagbodydo);
				saga = _resourceWrapper.Unwrap<ISaga>(sagaRes);
			}
			else
			{
				saga = _resourceWrapper.Create<ISaga>(sagakind);
			}
			return new PickedSaga(sagaId, saga, new ServiceBusItem(message));
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

		public async Task ReleaseSaga(PickedSaga picked)
		{
			if (!picked.Available)
				throw new InvalidOperationException("Saga is not picked");
			if (picked.Id.HasValue)
				await SagaUpdate(picked.Saga, picked.Id.Value);
		}

		public async Task SendMessage(IServiceBusItem item)
		{
			await SaveSagaMap();

			var msg = _resourceWrapper.Wrap(item.Message).Store(_resourceWrapper);
			var json = DynamicObjectConverters.ToJson(msg);
			var prm = new DynamicObject() {
				{ "Body", json },
				{ "Id", (Int64)0 },
				{ "Kind", msg.Get<String>("$res") },
				{ "CorrelationId", item.Message.CorrelationId.ToString() }
			};

			await _dbContext.ExecuteExpandoAsync(null, $"{Schema}.[Message.Send]", prm);
			// TODO: msgId ????
			Int64 msgId = prm.Get<Int64>("Id");
			if (item.After != null)
			{
				foreach (var a in item.After)
				{
					var aftermsg = _resourceWrapper.Wrap(a.Message).Store(_resourceWrapper);
					var afterjson = DynamicObjectConverters.ToJson(aftermsg);

					var prmafter = new DynamicObject() {
						{"Parent", msgId},
						{ "Body", afterjson },
						{ "Id", (Int64)0 },
						{ "Kind", aftermsg.Get<String>("$res") },
						{ "CorrelationId", a.Message.CorrelationId.ToString() }
					};

					await _dbContext.ExecuteExpandoAsync(null, $"{Schema}.[Message.Send]", prmafter);
				}
			}
		}

		Task SagaUpdate(ISaga saga, Guid id)
		{
			var body = DynamicObjectConverters.ToJson(saga.Store(_resourceWrapper));
			var prms = new DynamicObject()
			{
				{ "Id", id },
				{ "CorrelationId", saga.CorrelationId.ToString()},
				{ "Body", body },
				{ "IsComplete", saga.IsComplete }
			};
			return _dbContext.ExecuteExpandoAsync(null, $"{Schema}.[Saga.Update]", prms);
		}


		async Task SaveSagaMap()
		{
			if (_sagaMapSaved)
				return;
			var list = _sagaResolver.GetMap().Select(x => new SagaMapItem(x.Key, x.Value.SagaKind));
			await _dbContext.SaveListAsync<SagaMapItem>(null, $"{Schema}.[SagaMap.Save]", new { Host = _host }, list);
			_sagaMapSaved = true;
		}

	}
}
