// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using A2v10.ProcS.Interfaces;

namespace A2v10.ProcS
{
	public interface IStartWithMessage<T>
	{
		Task Start(T message);
	}

	public interface IHandleMessage<T>
	{
		void Handle(T message);
	}

	public interface ISaga
	{
		Boolean IsComplete { get; }
		Guid Id { get; }
	}

	public class CallApiRequest
	{
		public String Url { get; set; }
	}


	public class ResumeProcessMessage : IDomainEvent
	{
		public Guid Id { get; }
	}

	public interface IDomainEvent
	{
		Guid Id { get; }
	}

	public class CallHttpApiSaga: ISaga,
		IStartWithMessage<CallApiRequest>
	{

		private readonly IServiceBus _serviceBus;

		public CallHttpApiSaga(IServiceBus serviceBus)
		{
			_serviceBus = serviceBus;
		}

		public Guid Id { get; set; }
		public Boolean IsComplete => true;

		public async Task Start(CallApiRequest message)
		{
			await Task.Delay(1000);

			var resumeProcess = new ResumeProcessMessage();
			_serviceBus.Send(resumeProcess);
		}

		public static void Register()
		{
			WorkflowServiceBus.RegisterSaga<CallApiRequest, CallHttpApiSaga>();
		}
	}

	public class CallHttpApi : IWorkflowAction
	{
		public String Url { get; set; }
		public String Method { get; set; }

		async public Task<ActionResult> Execute(IWorkflowExecuteContext context)
		{
			await context.SaveInstance();
			//var url = context.Resolve(Url);
			var request = new CallApiRequest();
			request.Url = Url;
			//context.SendMessage(request);
			//context.ScheduleAction("CallApi")
			//result = await GetWeather("");
			return ActionResult.Idle;
		}
	}
}
