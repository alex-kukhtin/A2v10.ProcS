// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;
using System.Threading.Tasks;
using A2v10.ProcS.Interfaces;

namespace A2v10.ProcS
{
	public class ResumeProcess : IMessage
	{
		public String CorrelationId { get; }

		public Guid Id { get; }
		public String Result { get; }

		public ResumeProcess(Guid id, String result)
		{
			Id = id;
			Result = result;
		}
	}

	public class ProcessSaga : ISaga
	{
		public Boolean IsComplete => false;

		public static void Register()
		{
			ServiceBus.RegisterSaga<ResumeProcess, ProcessSaga>();
		}

		#region dispatch
		public Task<String> Handle(IHandleContext context, IMessage message)
		{
			switch (message)
			{
				case ResumeProcess resumeProcess:
					return HandleResume(context, resumeProcess);
			}
			return Task.FromResult<String>(null);
		}
		#endregion

		public async Task<String> HandleResume(IHandleContext context, ResumeProcess message)
		{
			var instance = await context.LoadInstance(message.Id);
			var resumeContext = context.CreateResumeContext(instance);
			resumeContext.Result = message.Result;
			await instance.Workflow.Resume(resumeContext);
			return null;
		}
	}
}
