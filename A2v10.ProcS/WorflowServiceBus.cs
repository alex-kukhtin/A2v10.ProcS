// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using A2v10.ProcS.Interfaces;

namespace A2v10.ProcS
{
	public class WorkflowServiceBus : IServiceBus
	{
		private static readonly Dictionary<Type, Type> _starters = new Dictionary<Type, Type>();
		private static readonly Dictionary<Guid, ISaga> _sagas = new Dictionary<Guid, ISaga>();

		ConcurrentQueue<Object> _messages = new ConcurrentQueue<Object>();

		public void Send(Object message) 
		{
			_messages.Enqueue(message);
		}

		public static void RegisterSaga<TStartMessage, TSaga>()
		{
			_starters.Add(typeof(TStartMessage), typeof(TSaga));
		}

		public async Task Run()
		{
			while (_messages.TryDequeue(out Object message))
			{
				await ProcessMessage(message);
			}
		}

		async Task ProcessMessage(Object message)
		{
			// domain messages
			if (message is IDomainEvent)
				await ProcessDomainEvent(message);
			// try to start new saga
			if (_starters.TryGetValue(message.GetType(), out Type sagaType)) {
				await StartSaga(sagaType, message);
				return;
			}
			// process all sagas
			await ProcessAllSagas(message);
		}

		async Task ProcessDomainEvent(Object message)
		{
			foreach (var saga in _sagas)
			{
				var sagaType = saga.GetType();
				var mi = sagaType.GetMethod("Handle");
				mi = mi.MakeGenericMethod(typeof(IDomainEvent));
				await (Task<Object>)mi.Invoke(saga, new Object[] { message });
			}
		}

		async Task StartSaga(Type sagaType, Object message)
		{
			var messageType = message.GetType();
			var saga = System.Activator.CreateInstance(sagaType, this) as ISaga;
			var mi = sagaType.GetMethod("Start");
			mi = mi.MakeGenericMethod(messageType);
			var result = mi.Invoke(saga, new Object[] { this, message });
			if (!saga.IsComplete)
				_sagas.Add(saga.Id, saga);
			await (Task<Object>) result;
		}

		Task ProcessAllSagas(Object message)
		{
			foreach (var s in _sagas)
			{
				ISaga saga = s.Value;

			}
			return Task.FromResult(0);
		}

	}
}
