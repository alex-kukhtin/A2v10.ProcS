// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Threading.Tasks;
using A2v10.ProcS.Infrastructure;

namespace A2v10.ProcS
{

	public class ContinueActivityMessage : MessageBase<Guid>
	{
		public Guid InstanceId { get; }
		public IDynamicObject Result { get; }
		public String Bookmark { get; }

		public ContinueActivityMessage(Guid instanceId, String bookmark, IDynamicObject result): base(instanceId)
		{
			InstanceId = instanceId;
			Bookmark = bookmark;
			Result = result;
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
