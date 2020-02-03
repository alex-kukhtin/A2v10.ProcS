// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

using A2v10.ProcS.Interfaces;

namespace A2v10.ProcS
{
	public class ServiceBus : IServiceBus
	{
		private static readonly Dictionary<Type, Type> _messagesMap = new Dictionary<Type, Type>();
		private static readonly Dictionary<Tuple<Type, String>, ISaga> _sagas = new Dictionary<Tuple<Type, String>, ISaga>();
		ConcurrentQueue<IMessage> _messages = new ConcurrentQueue<IMessage>();


		private readonly IInstanceStorage _instanceStorage;

		public ServiceBus(IInstanceStorage instanceStorage)
		{
			_instanceStorage = instanceStorage ?? throw new ArgumentNullException(nameof(instanceStorage));
		}

		public void Send(IMessage message)
		{
			_messages.Enqueue(message);
		}

		public static void RegisterSaga<TStartMessage, TSaga>() where TStartMessage : IMessage where TSaga : ISaga
		{
			_messagesMap.Add(typeof(TStartMessage), typeof(TSaga));
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
			if (_messagesMap.TryGetValue(message.GetType(), out Type sagaType))
				await ProcessMessage(sagaType, message);
		}

		async Task ProcessMessage(Type sagaType, IMessage message)
		{
			var key = Tuple.Create(sagaType, message.CorrelationId);
			if (message.CorrelationId != null && _sagas.TryGetValue(key, out ISaga saga))
			{
				await saga.Handle(message);
				if (saga.IsComplete)
					_sagas.Remove(Tuple.Create(sagaType, message.CorrelationId));
			}
			else 
			{
				saga = System.Activator.CreateInstance(sagaType, this, _instanceStorage) as ISaga;
				var corrId = await saga.Handle(message);
				if (corrId != null)
				{
					_sagas.Add(Tuple.Create(sagaType, corrId), saga);
				}
			}
		}
	}
}
