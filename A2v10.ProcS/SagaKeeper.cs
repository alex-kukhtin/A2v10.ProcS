// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
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
		public int HoldLevel;
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

		public ISaga GetSagaForMessage(IMessage message, out ISagaKeeperKey key, out Boolean isNew)
		{
			var sagaFactory = sagaResolver.GetSagaFactory(message.GetType());
			key = new SagaKeeperKey(sagaFactory.SagaKind, message.CorrelationId);
			if (message.CorrelationId != null && sagas.TryGetValue(key, out SagaState state))
			{
				isNew = false;
				if (Interlocked.CompareExchange(ref state.HoldLevel, 1, 0) == 0)
				    return state.Saga;
				return null;
			}
			else 
			{
				isNew = true;
				return sagaFactory.CreateSaga();
			}
		}

		public void SagaUpdate(ISaga saga, ISagaKeeperKey key)
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
	}
}
