// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;
using System.Net.Http;
using System.Threading.Tasks;
using A2v10.ProcS.Interfaces;

namespace A2v10.ProcS
{

	public class WaitCallbackMessage : MessageBase<String>
	{
		public WaitCallbackMessage(String tag) : base(tag)
		{
			Tag = tag;
		}

		public String Tag { get; set; }
		public String CorrelationExpression { get; set; }
	}

	public class WaitCallbackMessageProcess : MessageBase<String>
	{
		public WaitCallbackMessageProcess(Guid id, String tag, String corrVal) : base(tag + ":" + corrVal)
		{
			Id = id;
			Tag = tag;
			CorrelationValue = corrVal;
		}

		public Guid Id { get; set; }

		public String Tag { get; set; }
		public String CorrelationValue { get; set; }
		public String CorrelationExpression { get; set; }
	}

	public class CallbackMessage : MessageBase<String>
	{
		public CallbackMessage(String tag) : base(tag)
		{
			
		}

		public String Result { get; set; }
	}

	public class CallbackMessageResume : MessageBase<String>
	{
		public CallbackMessageResume(String tag, String corrVal) : base(tag + ":" + corrVal)
		{

		}

		public String Result { get; set; }
	}

	public class WaitApiCallbackSaga : SagaBase<String>
	{
		// serializable
		private String tag;
		private String correlationExpression;

		#region dispatch
		public override async Task Handle(IHandleContext context, IMessage message)
		{
			switch (message)
			{
				case WaitCallbackMessage wait:
					await Handle(context, wait);
					break;
				case CallbackMessage callback:
					await Handle(context, callback);
					break;
				default:
					throw new ArgumentOutOfRangeException(message.GetType().FullName);
			}
		}
		#endregion

		public Task Handle(IHandleContext context, WaitCallbackMessage message)
		{
			correlationExpression = message.CorrelationExpression;
			tag = message.Tag;
			CorrelationId.Value = message.CorrelationId.Value;
			return Task.CompletedTask;
		}

		public Task Handle(IHandleContext context, CallbackMessage message)
		{
			var json = message.Result;
			var se = context.ScriptContext;

			context.ScriptContext.SetValueFromJson("result", message.Result);
			var cval = se.Eval<String>(correlationExpression);

			var resumeProcess = new CallbackMessageResume(tag, cval);
			resumeProcess.Result = message.Result;
			context.SendMessage(resumeProcess);
			return Task.CompletedTask;
		}

		public static void Register()
		{
			InMemorySagaKeeper.RegisterMessageType<WaitCallbackMessage, WaitApiCallbackSaga>();
			InMemorySagaKeeper.RegisterMessageType<CallbackMessage, WaitApiCallbackSaga>();
		}
	}

	public class WaitApiCallbackProcessSaga : SagaBase<String>
	{
		// serializable
		private Guid _id;

		#region dispatch
		public override async Task Handle(IHandleContext context, IMessage message)
		{
			switch (message)
			{
				case WaitCallbackMessageProcess wait:
					await Handle(context, wait);
					break;
				case CallbackMessageResume callback:
					await Handle(context, callback);
					break;
				default:
					throw new ArgumentOutOfRangeException(message.GetType().FullName);
			}
		}
		#endregion

		public Task Handle(IHandleContext context, WaitCallbackMessageProcess message)
		{
			_id = message.Id;
			CorrelationId.Value = message.CorrelationId.Value;
			return Task.CompletedTask;
		}

		public Task Handle(IHandleContext context, CallbackMessageResume message)
		{
			var resumeProcess = new ResumeProcess(_id, message.Result);
			context.SendMessage(resumeProcess);
			IsComplete = true;
			return Task.CompletedTask;
		}

		public static void Register()
		{
			InMemorySagaKeeper.RegisterMessageType<WaitCallbackMessageProcess, WaitApiCallbackProcessSaga>();
			InMemorySagaKeeper.RegisterMessageType<CallbackMessageResume, WaitApiCallbackProcessSaga>();
		}
	}
}
