// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using A2v10.ProcS.Infrastructure;

namespace A2v10.ProcS
{
	internal class ServiceBusItem
	{
		public ServiceBusItem(IMessage message)
		{
			Message = message;
			After = new ServiceBusItem[0];
		}

		public ServiceBusItem(IMessage message, IEnumerable<ServiceBusItem> after)
		{
			Message = message;
			if (after == null) After = new ServiceBusItem[0];
			else After = after.ToArray();
		}

		public IMessage Message { get; private set; }
		public ServiceBusItem[] After { get; private set; }
	}

	public class InMemoryServiceBus : IServiceBus
	{
		private readonly ISagaKeeper _sagaKeeper;
		private readonly IScriptEngine _scriptEngine;
		private readonly ConcurrentQueue<ServiceBusItem> _messages = new ConcurrentQueue<ServiceBusItem>();
		private readonly ITaskManager _taskManager;

		private readonly IRepository _repository;

		private void SignalUpdate()
		{
			mre.Set();
		}

		private void Send(ServiceBusItem item)
		{
			_messages.Enqueue(item);
			SignalUpdate();
		}

		private void Send(IEnumerable<ServiceBusItem> items)
		{
			foreach (var itm in items) Send(itm);
			SignalUpdate();
		}

		public InMemoryServiceBus(ITaskManager taskManager, ISagaKeeper sagaKeeper, IRepository repository, IScriptEngine scriptEngine)
		{
			_taskManager = taskManager;
			_repository = repository ?? throw new ArgumentNullException(nameof(_repository));
			_scriptEngine = scriptEngine ?? throw new ArgumentNullException(nameof(scriptEngine));
			_sagaKeeper = sagaKeeper;
		}

		public void Send(IEnumerable<IMessage> messages)
		{
			Send(messages.Select(m => new ServiceBusItem(m)));
		}

		public void Send(IMessage message)
		{
			Send(new ServiceBusItem(message));
		}

		private static ServiceBusItem GetSequenceItem(IEnumerator<IMessage> en)
		{
			if (en.MoveNext())
			{
				var curr = en.Current;
				var after = GetSequenceItem(en);
				if (after == null) return new ServiceBusItem(curr);
				return new ServiceBusItem(curr, new ServiceBusItem[] { after });
			}
			return null;
		}

		public void SendSequence(IEnumerable<IMessage> messages)
		{
			var en = messages.GetEnumerator();
			Send(GetSequenceItem(en));
		}

		private readonly ManualResetEvent mre = new ManualResetEvent(false);

		public void Process()
		{
			while (_messages.TryDequeue(out var message))
			{
				if (!ProcessItem(message)) _messages.Enqueue(message);
			}
		}

		private volatile Boolean running = false;

		public void Stop()
		{
			running = false;
			SignalUpdate();
		}

		public void Start()
		{
			lock (this)
			{
				running = true;
				while (running)
				{
					Process();
					mre.Reset();
					mre.WaitOne(50);
				}
			}
		}

		private Boolean ProcessItem(ServiceBusItem item)
		{
			var saga = _sagaKeeper.GetSagaForMessage(item.Message, out ISagaKeeperKey key, out Boolean isNew);
			if (saga == null)
				return false;
			Func<Task> task = async () =>
			{
				using (var scriptContext = _scriptEngine.CreateContext())
				{
					var hc = new HandleContext(this, _repository, scriptContext);
					await saga.Handle(hc, item.Message);
				}
				Send(item.After);
				_sagaKeeper.SagaUpdate(saga, key);
			};
			_taskManager.AddTask(task);
			return true;
		}
	}
}
