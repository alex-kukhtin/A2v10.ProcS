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
	public class ServiceBusItem : IServiceBusItem
	{
		public ServiceBusItem(IMessage message)
		{
			Message = message;
			After = Array.Empty<ServiceBusItem>();
		}

		public ServiceBusItem(IMessage message, IEnumerable<ServiceBusItem> after)
		{
			Message = message;
			if (after == null) 
				After = Array.Empty<ServiceBusItem>();
			else After = after.ToArray();
		}

		public IMessage Message { get; private set; }
		public IServiceBusItem[] After { get; private set; }
	}

	public class InMemoryBus : IMessageBus
	{
		private readonly ConcurrentQueue<IServiceBusItem> _messages = new ConcurrentQueue<IServiceBusItem>();

		public Task Send(IServiceBusItem item)
		{
			_messages.Enqueue(item);
			return Task.CompletedTask;
		}

		public Task<Boolean> Pick(Func<IServiceBusItem, Boolean> proc)
		{
			if (_messages.TryDequeue(out var message))
			{
				if (!proc(message)) _messages.Enqueue(message);
				return Task.FromResult(true);
			}
			return Task.FromResult(false);
		}
	}

	public abstract class ServiceBusBase : IServiceBus
	{
		private readonly ISagaKeeper _sagaKeeper;
		private readonly IScriptEngine _scriptEngine;
		private readonly IMessageBus _bus = new InMemoryBus();
		private readonly ITaskManager _taskManager;

		private readonly IRepository _repository;

		protected ServiceBusBase(ITaskManager taskManager, ISagaKeeper sagaKeeper, IRepository repository, IScriptEngine scriptEngine)
		{
			_taskManager = taskManager;
			_repository = repository ?? throw new ArgumentNullException(nameof(_repository));
			_scriptEngine = scriptEngine ?? throw new ArgumentNullException(nameof(scriptEngine));
			_sagaKeeper = sagaKeeper;
		}

		protected abstract void SignalUpdate();

		private IPromise SendInternal(IServiceBusItem item)
		{
			async Task task()
			{
				await _bus.Send(item);
			}
			return _taskManager.AddTask(task);
		}

		protected void Send(IServiceBusItem item)
		{
			SendInternal(item).Done(SignalUpdate);
		}

		protected void Send(IEnumerable<IServiceBusItem> items)
		{
			var pa = new PromiseAggregator(items.Select(itm => SendInternal(itm)));
			_taskManager.AddTask(pa.Aggregate).Done(SignalUpdate);
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

		public Task Process()
		{
			return Process(CancellationToken.None);
		}

		public async Task Process(CancellationToken token)
		{
			while (!token.IsCancellationRequested && await _bus.Pick(ProcessItem)) ;
		}

		private Boolean ProcessItem(IServiceBusItem item)
		{
			var saga = _sagaKeeper.GetSagaForMessage(item.Message, out ISagaKeeperKey key, out Boolean isNew);
			if (saga == null)
				return false;
			async Task task()
			{
				using (var scriptContext = _scriptEngine.CreateContext())
				{
					var hc = new HandleContext(this, _repository, scriptContext);
					await saga.Handle(hc, item.Message);
				}
				Send(item.After);
				_sagaKeeper.SagaUpdate(saga, key);
			}
			_taskManager.AddTask(task);
			return true;
		}
	}

	public class ServiceBus : ServiceBusBase
	{
		public ServiceBus(ITaskManager taskManager, ISagaKeeper sagaKeeper, IRepository repository, IScriptEngine scriptEngine)
			: base(taskManager, sagaKeeper, repository, scriptEngine)
		{
			
		}

		protected override void SignalUpdate()
		{
			mre.Set();
		}

		private readonly ManualResetEvent mre = new ManualResetEvent(false);

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
					Process(CancellationToken.None).Wait();
					mre.Reset();
					mre.WaitOne(50);
				}
			}
		}
	}

	public class ServiceBusAsync : ServiceBusBase
	{
		public ServiceBusAsync(ITaskManager taskManager, ISagaKeeper sagaKeeper, IRepository repository, IScriptEngine scriptEngine)
			: base(taskManager, sagaKeeper, repository, scriptEngine)
		{

		}

		protected override void SignalUpdate()
		{
			rwl.AcquireReaderLock(10);
			try
			{
				ts?.TrySetResult(true);
			}
			finally
			{
				rwl.ReleaseReaderLock();
			}
		}

		private readonly ReaderWriterLock rwl = new ReaderWriterLock();
		private volatile TaskCompletionSource<bool> ts = null;

		public async Task Run(CancellationToken token)
		{
			while (!token.IsCancellationRequested)
			{
				await Process(token);
				rwl.AcquireWriterLock(0);
				try
				{
					ts = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
				}
				finally
				{
					rwl.ReleaseWriterLock();
				}
				await Task.WhenAny(ts.Task, Task.Delay(50, token));
			}
		}
	}
}
