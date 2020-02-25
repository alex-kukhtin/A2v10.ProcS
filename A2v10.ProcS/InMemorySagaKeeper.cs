// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Collections.Concurrent;
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

		private ISaga GetSagaForMessage(IMessage message, out ISagaKeeperKey key, out Boolean isNew)
		{
			var sagaFactory = sagaResolver.GetSagaFactory(message.GetType());
			key = new SagaKeeperKey(sagaFactory.SagaKind, message.CorrelationId);
			isNew = false;
			var state = sagas.GetOrAdd(key, k =>
			{
				var saga = sagaFactory.CreateSaga();
				return new SagaState(saga);
			});
			if (Interlocked.CompareExchange(ref state.HoldLevel, 1, 0) == 0)
				return state.Saga;
			return null;
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

		private readonly ConcurrentQueue<IServiceBusItem> _messages = new ConcurrentQueue<IServiceBusItem>();

		public Task SendMessage(IServiceBusItem item)
		{
			_messages.Enqueue(item);
			return Task.CompletedTask;
		}

		public Task<PickedSaga> PickSaga()
		{
			while (_messages.TryDequeue(out var message))
			{
				var saga = GetSagaForMessage(message.Message, out ISagaKeeperKey key, out Boolean isNew);
				if (saga == null)
				{
					_messages.Enqueue(message);
					continue;
				}
				return Task.FromResult(new PickedSaga(key, saga, message));
			}
			return Task.FromResult(new PickedSaga(false));
		}

		public Task ReleaseSaga(PickedSaga picked)
		{
			if (!picked.Available) throw new Exception("Saga is not picked");
			SagaUpdate(picked.Saga, picked.Key);
			return Task.CompletedTask;
		}
	}
}
