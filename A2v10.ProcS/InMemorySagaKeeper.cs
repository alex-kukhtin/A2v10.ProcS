// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using A2v10.ProcS.Infrastructure;

namespace A2v10.ProcS
{
	public class SagaKeeperKey : ISagaKeeperKey
	{
		public String SagaKind { get; private set; }
		public ICorrelationId CorrelationId { get; private set; }


		public SagaKeeperKey(String sagaKind, ICorrelationId correlationId)
		{
			SagaKind = sagaKind;
			CorrelationId = correlationId;
		}

		public SagaKeeperKey(ISaga saga)
		{
			SagaKind = saga.Kind;
			CorrelationId = saga.CorrelationId;
		}

		public override Int32 GetHashCode()
		{
			return SagaKind.GetHashCode() + 17 * (CorrelationId?.GetHashCode() ?? 0);
		}

		public override String ToString()
		{
			return SagaKind + ":" + (CorrelationId?.ToString() ?? "{null}");
		}

		public Boolean Equals(ISagaKeeperKey other)
		{
			return SagaKind.Equals(other.SagaKind) && CorrelationId.Equals(other.CorrelationId);
		}
	}

	public class SagaState
	{
		public SagaState(ISaga saga)
		{
			Saga = saga;
			HoldLevel = 0;
		}

		public ISaga Saga { get; }
		public Int32 HoldLevel;
		public Exception FailedException;
	}

	public class InMemorySagaKeeper : ISagaKeeper
	{
		private readonly ISagaResolver sagaResolver;
		private readonly ConcurrentDictionary<ISagaKeeperKey, SagaState> sagas;

		public InMemorySagaKeeper(ISagaResolver sagaResolver)
		{
			sagas = new ConcurrentDictionary<ISagaKeeperKey, SagaState>();
			this.sagaResolver = sagaResolver;
		}

		private (ISagaKeeperKey key, ISaga saga) GetSagaForMessage(IMessage message)
		{
			var sagaFactory = sagaResolver.GetSagaFactory(message.GetType());
			var key = new SagaKeeperKey(sagaFactory.SagaKind, message.CorrelationId);
			var state = sagas.GetOrAdd(key, k =>
			{
				var saga = sagaFactory.CreateSaga();
				return new SagaState(saga);
			});
			var hst = Interlocked.CompareExchange(ref state.HoldLevel, 1, 0);
			if (hst == 0)
				return (key, state.Saga);
			return default;
		}

		private void SagaUpdate(ISaga saga, ISagaKeeperKey key)
		{
			if (saga.IsComplete || saga.CorrelationId == null || !saga.CorrelationId.Equals(key.CorrelationId))
			{
				sagas.TryRemove(key, out var removed);
			}
			if (!saga.IsComplete)
			{
				var ns = new SagaState(saga);
				sagas.AddOrUpdate(new SagaKeeperKey(saga), ns, (k, s) => ns);
			}
		}

		private void SagaFailed(ISaga saga, ISagaKeeperKey key, Exception exception)
		{
			var ns = new SagaState(saga)
			{
				HoldLevel = 2,
				FailedException = exception
			};
			sagas.AddOrUpdate(key, ns, (k, s) => ns);
		}

		private readonly ConcurrentQueue<IServiceBusItem> _messages = new ConcurrentQueue<IServiceBusItem>();

		public Task SendMessage(IServiceBusItem item)
		{
			_messages.Enqueue(item);
			return Task.CompletedTask;
		}

		private readonly Dictionary<Guid, ISagaKeeperKey> picked = new Dictionary<Guid, ISagaKeeperKey>();

		public Task<PickedSaga> PickSaga()
		{
			while (_messages.TryDequeue(out var message))
			{
				var saga = GetSagaForMessage(message.Message);
				if (saga == default)
				{
					_messages.Enqueue(message);
					continue;
				}
				var id = Guid.NewGuid();
				picked.Add(id, saga.key);
				return Task.FromResult(new PickedSaga(id, saga.saga, message));
			}
			return Task.FromResult(new PickedSaga(false));
		}

		public Task ReleaseSaga(PickedSaga picked)
		{
			if (picked.Id == null)
				return Task.CompletedTask;
			if (!picked.Available || !this.picked.ContainsKey(picked.Id.Value)) 
				throw new Exception("Saga is not picked");
			SagaUpdate(picked.Saga, this.picked[picked.Id.Value]);
			return Task.CompletedTask;
		}

		public Task FailSaga(PickedSaga picked, Exception exception)
		{
			if (picked.Id == null)
				return Task.CompletedTask;
			if (!picked.Available || !this.picked.ContainsKey(picked.Id.Value))
				throw new Exception("Saga is not picked");
			SagaFailed(picked.Saga, this.picked[picked.Id.Value], exception);
			return Task.CompletedTask;
		}
	}
}
