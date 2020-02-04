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

		/*public CorrelationId()
		{
			Value = default;
		}*/

		public CorrelationId(T value)
		{
			Value = value;
		}

		public Boolean Equals(ICorrelationId other)
		{
			if (other is CorrelationId<T> tt)
				return Equals(tt);
			return false;
		}

		public override Int32 GetHashCode()
		{
			return Value?.GetHashCode() ?? 0;
		}

		public Boolean Equals(CorrelationId<T> other)
		{
			if (Value == null) return other.Value == null;
			return Value.Equals(other.Value);
		}
	}

	public class MessageBase<CorrelationT> : IMessage where CorrelationT : IEquatable<CorrelationT>
	{
		public MessageBase(CorrelationT correlationId)
		{
			CorrelationId = new CorrelationId<CorrelationT>(correlationId);
		}

		public CorrelationId<CorrelationT> CorrelationId { get; }

		ICorrelationId IMessage.CorrelationId => CorrelationId;
	}

	public class ServiceBus : IServiceBus
	{
		private readonly ISagaKeeper _sagaKeeper;
		private readonly IScriptEngine _scriptEngine;
		private readonly ConcurrentQueue<IMessage> _messages = new ConcurrentQueue<IMessage>();


		private readonly IInstanceStorage _instanceStorage;

		public ServiceBus(ISagaKeeper sagaKeeper, IInstanceStorage instanceStorage, IScriptEngine scriptEngine)
		{
			_instanceStorage = instanceStorage ?? throw new ArgumentNullException(nameof(instanceStorage));
			_scriptEngine = scriptEngine ?? throw new ArgumentNullException(nameof(scriptEngine));
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

		async Task ProcessMessage(IMessage message)
		{
			var saga = _sagaKeeper.GetSagaForMessage(message, out ISagaKeeperKey key, out Boolean isNew);

			using (var scriptContext = _scriptEngine.CreateContext())
			{
				var hc = new HandleContext(this, _instanceStorage, scriptContext);
				await saga.Handle(hc, message);
			}

			_sagaKeeper.SagaUpdate(saga, key);
		}
	}
}
