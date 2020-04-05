// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Threading.Tasks;
using A2v10.ProcS.Infrastructure;

namespace A2v10.ProcS
{
	[ResourceKey(ukey)]
	public class SetBookmarkMessage : MessageBase<Guid>
	{
		public const String ukey = ProcS.ResName + ":" + nameof(SetBookmarkMessage);

		[RestoreWith]
		public SetBookmarkMessage(Guid correlationId) : base(correlationId)
		{
		}
		public SetBookmarkMessage(Guid id, IResultMessage resultMessage) : this(id)
		{
			ResultMessage = resultMessage;
		}

		public IResultMessage ResultMessage { get; private set; }

		public override void Store(IDynamicObject store, IResourceWrapper wrapper)
		{
			var dres = wrapper.Wrap(ResultMessage).Store(wrapper);
			store.Set(nameof(ResultMessage), dres);
		}

		public override void Restore(IDynamicObject store, IResourceWrapper wrapper)
		{
			var dynres = store.GetDynamicObject(nameof(ResultMessage));
			ResultMessage = wrapper.Unwrap<IResultMessage>(dynres);
		}
	}

	[ResourceKey(ukey)]
	public class ResumeBookmarkMessage : MessageBase<Guid>
	{
		public const String ukey = ProcS.ResName + ":" + nameof(ResumeBookmarkMessage);

		[RestoreWith]
		public ResumeBookmarkMessage(Guid correlationId) : base(correlationId)
		{
			Id = correlationId;
		}

		public ResumeBookmarkMessage(Guid correlationId, IDynamicObject result) : this(correlationId)
		{
			Id = correlationId;
			Result = result;
		}

		public Guid Id { get; private set; }
		public IDynamicObject Result { get; private set; }

		public override void Store(IDynamicObject store, IResourceWrapper _)
		{
			store.Set(nameof(Id), Id);
			store.Set(nameof(Result), Result);
		}

		public override void Restore(IDynamicObject store, IResourceWrapper _)
		{
			Id = store.Get<Guid>(nameof(Id));
			Result = store.GetDynamicObject(nameof(Result));
		}
	}

	public class BookmarkSaga : SagaBaseDispatched<Guid, SetBookmarkMessage, ResumeBookmarkMessage>
	{
		public const String ukey = ProcS.ResName + ":" + nameof(BookmarkSaga);

		public BookmarkSaga() : base(ukey)
		{
		}

		private IResultMessage resultMessage;

		protected override Task Handle(IHandleContext context, SetBookmarkMessage message)
		{
			SetCorrelation(message.CorrelationId);
			resultMessage = message.ResultMessage;
			return Task.CompletedTask;
		}

		protected override Task Handle(IHandleContext context, ResumeBookmarkMessage message)
		{
			if (resultMessage != null)
			{
				resultMessage.Result = message.Result;
				context.SendMessage(resultMessage);
			}
			IsComplete = true;
			return Task.CompletedTask;
		}

		public override IDynamicObject Store(IResourceWrapper wrapper)
		{
			var d = new DynamicObject();
			var dres = wrapper.Wrap(resultMessage).Store(wrapper);
			d.Set(nameof(resultMessage), dres);
			return d;
		}
		public override void Restore(IDynamicObject store, IResourceWrapper wrapper)
		{
			var dynmsg = store.GetDynamicObject(nameof(resultMessage));
			resultMessage =  wrapper.Unwrap<IResultMessage>(dynmsg);
		}
	}

	[ResourceKey(ukey)]
	public class ContinueActivityMessage : MessageBase<String>, IResultMessage
	{
		public const String ukey = ProcS.ResName + ":" + nameof(ContinueActivityMessage);

		public Guid InstanceId { get; private set; }
		public IDynamicObject Result { get; set; }
		public String Bookmark { get; private set; }

		[RestoreWith]
		public ContinueActivityMessage() : base(null)
		{
		}

		public ContinueActivityMessage(Guid instanceId, String bookmark, IDynamicObject result): base(null)
		{
			InstanceId = instanceId;
			Bookmark = bookmark;
			Result = result;
		}

		public ContinueActivityMessage(Guid instanceId, String bookmark) : base(null)
		{
			InstanceId = instanceId;
			Bookmark = bookmark;
		}

		public override void Store(IDynamicObject store, IResourceWrapper _)
		{
			store.Set(nameof(InstanceId), InstanceId);
			store.Set(nameof(Result), Result);
			store.Set(nameof(Bookmark), Bookmark);
		}

		public override void Restore(IDynamicObject store, IResourceWrapper _)
		{
			InstanceId = store.Get<Guid>(nameof(InstanceId));
			Result = store.GetDynamicObject(nameof(Result));
			Bookmark = store.Get<String>(nameof(Bookmark));
		}
	}

	[ResourceKey(ukey)]
	public class StartProcessMessage : MessageBase<String>
	{
		public const String ukey = ProcS.ResName + ":" + nameof(StartProcessMessage);

		public Guid ParentId { get; set; }
		public String ProcessId { get; set; }
		public IDynamicObject Parameters { get; set; }


		[RestoreWith]
		public StartProcessMessage() : base(null)
		{
		}

		public override void Store(IDynamicObject store, IResourceWrapper _)
		{
			store.Set(nameof(ParentId), ParentId);
			store.Set(nameof(ProcessId), ProcessId);
			store.Set(nameof(Parameters), Parameters);
		}

		public override void Restore(IDynamicObject store, IResourceWrapper _)
		{
			ParentId = store.Get<Guid>(nameof(ParentId));
			ProcessId = store.Get<String>(nameof(ProcessId));
			Parameters = store.GetDynamicObject(nameof(Parameters));
		}
	}

	public class ProcessSaga : SagaBaseDispatched<String, StartProcessMessage, ContinueActivityMessage>
	{
		public const String ukey = ProcS.ResName + ":" + nameof(ProcessSaga);

		protected readonly IRepository _repository;
		protected readonly IScriptEngine _scriptEngine;

		public ProcessSaga(IRepository repository, IScriptEngine scriptEngine) : base(ukey)
		{
			_repository = repository;
			_scriptEngine = scriptEngine;
		}

		protected override async Task Handle(IHandleContext context, StartProcessMessage message)
		{
			var instance = await _repository.CreateInstance(new Identity(message.ProcessId), message.ParentId);
			if (message.Parameters != null)
				instance.SetParameters(message.Parameters);
			using (var scriptContext = _scriptEngine.CreateContext())
			{
				var executeContext = new ExecuteContext(context.Bus, _repository, scriptContext, context.Logger, context.NotifyManager, instance);
				await instance.Workflow.Run(executeContext);
			}
		}

		protected async override Task Handle(IHandleContext context, ContinueActivityMessage message)
		{
			var instance = await _repository.Get(message.InstanceId);
			using (var scriptContext = _scriptEngine.CreateContext())
			{
				var continueContext = new ExecuteContext(context.Bus, _repository, scriptContext, context.Logger, context.NotifyManager, instance)
				{
					Bookmark = message.Bookmark,
					Result = message.Result
				};
				scriptContext.SetValue("reply", message?.Result ?? new DynamicObject());
				continueContext.IsContinue = true;
				await instance.Workflow.Continue(continueContext);
			}
		}
	}
}
