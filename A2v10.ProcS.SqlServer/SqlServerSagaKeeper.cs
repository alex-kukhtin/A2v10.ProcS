// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using A2v10.Data.Interfaces;
using A2v10.ProcS.Infrastructure;

namespace A2v10.ProcS.SqlServer
{
	public class Tracker
	{
		public void Track(String text)
		{
			//using var tw = new StreamWriter("d:\\temp\\log.txt", true);
			//tw.WriteLine(text);
		}
	}

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
		private readonly Tracker _tracker = new Tracker();

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
			String sagaCorrelationId = dobj.Get<String>("SagaCorrelationId");

			_tracker.Track("---------------");
			_tracker.Track($"Peek Message. type:{message.GetType()}, correlationId: {message.CorrelationId}");

			Guid? sagaId = dobj.Get<Guid?>("SagaId");


			var sagakind = dobj.Get<String>("SagaKind");
			var sagabodyjson = dobj.Get<String>("SagaBody");

			ISaga saga;
			if (!String.IsNullOrEmpty(sagabodyjson))
			{
				var sagbodydo = DynamicObjectConverters.FromJson(sagabodyjson);
				var sagaRes = new Resource(sagakind, sagbodydo);
				saga = _resourceWrapper.Unwrap<ISaga>(sagaRes);
				saga.CorrelationId.FromString(sagaCorrelationId);
				_tracker.Track($"Restore saga. id:{sagaId}, kind:{sagakind}, correlationId: {sagaCorrelationId}");
			}
			else
			{
				_tracker.Track($"Create saga. id:{sagaId}, kind:{sagakind}, correlationId: {sagaCorrelationId}");
				saga = _resourceWrapper.Create<ISaga>(sagakind, new DynamicObject());
			}
			return new PickedSaga(sagaId, saga, new ServiceBusItem(message));
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
				{ "After", item.After },
				{ "CorrelationId", item.Message.CorrelationId.ToString() }
			};

			_tracker.Track($"SendMessage. kind:{prm.Get<String>("Kind")}, correlationId: {item.Message.CorrelationId.ToString()}");

			await _dbContext.ExecuteExpandoAsync(null, $"{Schema}.[Message.Send]", prm);
			// TODO: msgId ????
			Int64 msgId = prm.Get<Int64>("Id");
			if (item.Next != null)
			{
				foreach (var a in item.Next)
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

					_tracker.Track($"SendMessage. kind:{prmafter.Get<String>("Kind")}, correlationId: {a.Message.CorrelationId.ToString()}");
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
			_tracker.Track($"Update saga. id: {id}, isComplete: {saga.IsComplete}, kind:{saga.Kind}, correlationId: {saga.CorrelationId.ToString()}");
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


		async public Task FailSaga(PickedSaga picked, Exception exception)
		{
			try
			{
				var correlationId = picked.ServiceBusItem?.Message?.CorrelationId?.ToString();
				var dobj = new DynamicObject()
				{
					{"Id", picked.Id },
					{"Exception",  exception.Message},
					{"SagaKind", picked.Saga?.Kind},
					{"StackTrace", exception.StackTrace },
					{"CorrelationId", correlationId }
				};
				_tracker.Track($"Fail saga. id:{picked.Id}, kind:{picked.Saga.Kind}, correlationId: {correlationId}");
				await _dbContext.ExecuteExpandoAsync(null, $"{Schema}.[Saga.Fail]", dobj);
			}
			catch (Exception ex)
			{
				LogSystemException(ex);
			}
		}

		void LogSystemException(Exception ex)
		{
			// TODO: (
		}
	}
}
