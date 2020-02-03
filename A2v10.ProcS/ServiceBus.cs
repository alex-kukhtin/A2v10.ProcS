// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

using A2v10.ProcS.Interfaces;

namespace A2v10.ProcS
{
	public class CorrelationId<T> : ICorrelationId, IEquatable<CorrelationId<T>> where T : IEquatable<T>
	{
		public T Value { get; set; }

		public CorrelationId()
		{
			Value = default(T);
		}

		public CorrelationId(T value)
		{
			Value = value;
		}

		public bool Equals(ICorrelationId other)
		{
			var tt = other as CorrelationId<T>;
			if (tt == null) return false;
			return Equals(tt);
		}

		public override int GetHashCode()
		{
			return Value?.GetHashCode() ?? 0;
		}

		public bool Equals(CorrelationId<T> other)
		{
			if (Value == null) return other.Value == null;
			return Value.Equals(other.Value);
		}
	}

	public class MessageBase<CorrelationT> : IMessage where CorrelationT : IEquatable<CorrelationT>
	{
		public CorrelationId<CorrelationT> CorrelationId { get; set; } = new CorrelationId<CorrelationT>();

		ICorrelationId IMessage.CorrelationId => CorrelationId;
	}

	public class ServiceBus : IServiceBus
	{
		private readonly ISagaKeeper _sagaKeeper;
		ConcurrentQueue<IMessage> _messages = new ConcurrentQueue<IMessage>();


		private readonly IInstanceStorage _instanceStorage;

		public ServiceBus(ISagaKeeper sagaKeeper, IInstanceStorage instanceStorage)
		{
			_instanceStorage = instanceStorage ?? throw new ArgumentNullException(nameof(instanceStorage));
			_sagaKeeper = sagaKeeper;
		}

		public void Send(IMessage message)
		{
			_messages.Enqueue(message);
		}

		public async Task Run()
		{
			while (_messages.TryDequeue(out IMessage message))
			{
				await ProcessMessage(message);
			}
		}

		IHandleContext CreateHandleContext()
		{
			return new HandleContext(this, _instanceStorage);
		}

		async Task ProcessMessage(IMessage message)
		{
			var saga = _sagaKeeper.GetSagaForMessage(message, out ISagaKeeperKey key, out bool isNew);

			var hc = CreateHandleContext();
			await saga.Handle(hc, message);

			_sagaKeeper.SagaUpdate(saga, key);
		}
	}
}
