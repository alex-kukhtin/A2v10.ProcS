// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;
using System.Threading.Tasks;
using A2v10.ProcS.Infrastructure;

namespace A2v10.ProcS
{


	public class ResumeProcess : MessageBase<String>
	{
		public Guid Id { get; }
		public IDynamicObject Result { get; }

		public ResumeProcess(Guid id, IDynamicObject result) : base(null)
		{
			Id = id;
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

	public class ProcessSaga : SagaBaseDispatched<String, ResumeProcess, StartProcessMessage>
	{
		public ProcessSaga() : base(nameof(ProcessSaga))
		{

		}

		protected override async Task Handle(IHandleContext context, ResumeProcess message)
		{
			var instance = await context.LoadInstance(message.Id);
			var resumeContext = context.CreateResumeContext(instance);
			resumeContext.Result = message.Result;
			await instance.Workflow.Resume(resumeContext);
		}

		protected override async Task Handle(IHandleContext context, StartProcessMessage message)
		{
			IInstance instance = await context.StartProcess(message.ProcessId, message.ParentId, message.Parameters);
			message.CorrelationId.Value = instance.Id;

		}
	}
}
