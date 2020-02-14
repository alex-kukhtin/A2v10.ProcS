// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
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
			lock (lck)
			{
				ts?.TrySetResult(true);
			}
		}

		private readonly Object lck = new Object();
        private volatile TaskCompletionSource<bool> ts = null;

		private readonly Lazy<CancellationTokenSource> cancelWhenEmpty = new Lazy<CancellationTokenSource>(() => new CancellationTokenSource());
		public CancellationTokenSource CancelWhenEmpty => cancelWhenEmpty.Value;

		public async Task Run(CancellationToken token)
		{
            while (!token.IsCancellationRequested)
			{
				while (!token.IsCancellationRequested && _messages.TryDequeue(out var message)) {
					await ProcessMessage(message);
				}
				if (cancelWhenEmpty.IsValueCreated) cancelWhenEmpty.Value.Cancel();
				lock (lck)
				{
					ts = new TaskCompletionSource<bool>();
				}
                await Task.WhenAny(ts.Task, Task.Delay(50, token));
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

        ~ServiceBus()
        {
			if (cancelWhenEmpty.IsValueCreated) cancelWhenEmpty.Value.Dispose();
		}
	}
}
