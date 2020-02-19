// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Threading.Tasks;
using A2v10.ProcS.Infrastructure;

namespace A2v10.ProcS
{
	public class SetBookmarkMessage : MessageBase<Guid>
	{
		public SetBookmarkMessage(Guid id, IResultMessage resultMessage) : base(id)
		{
			Id = id;
			ResultMessage = resultMessage;
		}

		public Guid Id { get; }
		public IResultMessage ResultMessage { get; }
	}

	public class ResumeBookmarkMessage : MessageBase<Guid>
	{
		public ResumeBookmarkMessage(Guid id, IDynamicObject result) : base(id)
		{
			Id = id;
			Result = result;
		}

		public Guid Id { get; }
		public IDynamicObject Result { get; }
	}

	public class BookmarkSaga : SagaBaseDispatched<Guid, SetBookmarkMessage, ResumeBookmarkMessage>
	{
		public BookmarkSaga() : base(nameof(BookmarkSaga))
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
			resultMessage.Result = message.Result;
			context.SendMessage(resultMessage);
			return Task.CompletedTask;
		}
	}

	public class ContinueActivityMessage : MessageBase<Guid>, IResultMessage
	{
		public Guid InstanceId { get; }
		public IDynamicObject Result { get; set; }
		public String Bookmark { get; }

		public ContinueActivityMessage(Guid instanceId, String bookmark, IDynamicObject result): base(instanceId)
		{
			InstanceId = instanceId;
			Bookmark = bookmark;
			Result = result;
		}

		public ContinueActivityMessage(Guid instanceId, String bookmark) : base(instanceId)
		{
			InstanceId = instanceId;
			Bookmark = bookmark;
		}
	}

	public class StartProcessMessage : MessageBase<Guid>
	{
		public Guid ParentId { get; }
		public String ProcessId { get; set; }
		public IDynamicObject Parameters { get; set; }

		public StartProcessMessage(Guid parentId) : base(parentId)
		{
			ParentId = parentId;
		}
	}

	public class ProcessSaga : SagaBaseDispatched<String, StartProcessMessage, ContinueActivityMessage>
	{
		public ProcessSaga() : base(nameof(ProcessSaga))
		{
		}

		protected override async Task Handle(IHandleContext context, StartProcessMessage message)
		{
			IInstance instance = await context.StartProcess(message.ProcessId, message.ParentId, message.Parameters);
			message.CorrelationId.Value = instance.Id;
		}

		protected async override Task Handle(IHandleContext context, ContinueActivityMessage message)
		{
			var instance = await context.LoadInstance(message.InstanceId);
			var continueContext = context.CreateExecuteContext(instance, message.Bookmark, message.Result);
			continueContext.ScriptContext.SetValue("reply", message.Result);
			continueContext.IsContinue = true;
			await instance.Workflow.Continue(continueContext);
		}
	}
}
