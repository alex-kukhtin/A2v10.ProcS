// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

using A2v10.ProcS.Interfaces;

namespace A2v10.ProcS
{
	public abstract class SagaBase<CorrelationT> : ISaga where CorrelationT : IEquatable<CorrelationT>
	{
		public CorrelationId<CorrelationT> CorrelationId { get; } = new CorrelationId<CorrelationT>(default);

		ICorrelationId ISaga.CorrelationId => CorrelationId;

		public Boolean IsComplete { get; set; }
		public abstract Task Handle(IHandleContext context, IMessage message);
	}

	public class InMemorySagaKeeperKey : ISagaKeeperKey
	{
		public Type SagaType { get; private set; }
		public ICorrelationId CorrelationId { get; private set; }


		public InMemorySagaKeeperKey(Type sagaType, ICorrelationId correlationId)
		{
			SagaType = sagaType;
			CorrelationId = correlationId;
		}

		public InMemorySagaKeeperKey(ISaga saga)
		{
			SagaType = saga.GetType();
			CorrelationId = saga.CorrelationId;
		}

		public override Int32 GetHashCode()
		{
			return SagaType.GetHashCode() + 17 * (CorrelationId?.GetHashCode() ?? 0);
		}

		public override String ToString()
		{
			return SagaType.Name + ":" + (CorrelationId?.ToString() ?? "{null}");
		}

		public Boolean Equals(ISagaKeeperKey other)
		{
			return SagaType.Equals(other.SagaType) && CorrelationId.Equals(other.CorrelationId);
		}
	}

	public class InMemorySagaKeeper : ISagaKeeper
	{
		private static readonly Dictionary<Type, Type> _messagesMap = new Dictionary<Type, Type>();
		private readonly ConcurrentDictionary<ISagaKeeperKey, ISaga> _sagas = new ConcurrentDictionary<ISagaKeeperKey, ISaga>();
		
		public static void RegisterMessageType<TMessage, TSaga>() where TMessage : IMessage where TSaga : ISaga
		{
			_messagesMap.Add(typeof(TMessage), typeof(TSaga));
		}

		public ISaga GetSagaForMessage(IMessage message, out ISagaKeeperKey key, out Boolean isNew)
		{
			var sagaType = _messagesMap[message.GetType()];
			key = new InMemorySagaKeeperKey(sagaType, message.CorrelationId);
			if (message.CorrelationId != null && _sagas.TryGetValue(key, out ISaga saga))
			{
				isNew = false;
				return saga;
			}
			else 
			{
				isNew = true;
				return System.Activator.CreateInstance(sagaType) as ISaga;
			}
		}

		public void SagaUpdate(ISaga saga, ISagaKeeperKey key)
		{
			if (saga.IsComplete || saga.CorrelationId == null || !saga.CorrelationId.Equals(key.CorrelationId))
			{
				_sagas.TryRemove(key, out ISaga removed);
			}
			if (!saga.IsComplete)
			{
				_sagas.AddOrUpdate(new InMemorySagaKeeperKey(saga), saga, (k, s) => saga);
			}
		}
	}
}
