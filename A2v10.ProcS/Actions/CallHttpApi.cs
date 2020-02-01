// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using A2v10.ProcS.Interfaces;

namespace A2v10.ProcS
{
	/*
	public interface IStartWithMessage<T>
	{
		Task Start(T message);
	}

	public interface IHandleMessage<T>
	{
		Task Handle(T message);
	}
	*/

	public interface IMessage
	{
		Guid Id { get; }
	}

	public interface ISaga
	{
		Boolean IsComplete { get; }
		Guid Id { get; }

		Task Start(Object message);
		Task Handle(Object message);
	}

	public class CallApiRequest : IMessage
	{
		public Guid Id { get; set; }
		public String Url { get; set; }
	}

	public interface IDomainEvent
	{
		Guid Id { get; }
	}

	public class CallHttpApiSaga: SagaBase
	{
		public CallHttpApiSaga(Guid id, IServiceBus serviceBus, IInstanceStorage instanceStorage)
			:base(id, serviceBus, instanceStorage)
		{
		}

		#region dispatch
		public override Task Start(Object message)
		{
			switch (message)
			{
				case CallApiRequest apiRequest:
					return Start(apiRequest);
			}
			throw new ArgumentOutOfRangeException(message.GetType().FullName);
		}
		#endregion

		public async Task Start(CallApiRequest message)
		{
			await Task.Delay(1000);

			IsComplete = true;
			var resumeProcess = new ResumeProcess(Id);
			ServiceBus.Send(resumeProcess);
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

		async public Task<ActionResult> Execute(IExecuteContext context)
		{
			await context.SaveInstance();
			//var url = context.Resolve(Url);
			var request = new CallApiRequest();
			request.Id = context.Instance.Id;
			request.Url = Url;
			context.SendMessage(request);
			//context.ScheduleAction("CallApi")
			//result = await GetWeather("");
			return ActionResult.Idle;
		}
	}
}
