// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

using A2v10.ProcS.Infrastructure;

namespace A2v10.ProcS
{
	public class ServiceBus : IServiceBus
	{
		private readonly ISagaKeeper _sagaKeeper;
		private readonly IScriptEngine _scriptEngine;
		private readonly ConcurrentQueue<IMessage> _messages = new ConcurrentQueue<IMessage>();


		private readonly IRepository _repository;

		public ServiceBus(ISagaKeeper sagaKeeper, IRepository repository, IScriptEngine scriptEngine)
		{
			_repository = repository ?? throw new ArgumentNullException(nameof(_repository));
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
				var hc = new HandleContext(this, _repository, scriptContext);
				await saga.Handle(hc, message);
			}

			_sagaKeeper.SagaUpdate(saga, key);
		}
	}
}
