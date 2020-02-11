// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

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

	public class InMemorySagaKeeper : ISagaKeeper
	{
		private readonly ISagaManager sagaManager;
		private readonly ConcurrentDictionary<ISagaKeeperKey, ISaga> sagas;

		public InMemorySagaKeeper(ISagaManager sagaManager)
		{
			sagas = new ConcurrentDictionary<ISagaKeeperKey, ISaga>();
			this.sagaManager = sagaManager;
		}

		public ISaga GetSagaForMessage(IMessage message, out ISagaKeeperKey key, out Boolean isNew)
		{
			var sagaFactory = sagaManager.GetSagaFactory(message.GetType());
			key = new SagaKeeperKey(sagaFactory.SagaKind, message.CorrelationId);
			if (message.CorrelationId != null && sagas.TryGetValue(key, out ISaga saga))
			{
				isNew = false;
				return saga;
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
				sagas.TryRemove(key, out ISaga removed);
			}
			if (!saga.IsComplete)
			{
				sagas.AddOrUpdate(new SagaKeeperKey(saga), saga, (k, s) => saga);
			}
		}
	}
}
