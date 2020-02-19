// Copyright ©️ 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Net.Http;
using System.Threading.Tasks;
using A2v10.ProcS.Infrastructure;

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
		public WaitCallbackMessageProcess(Guid id, String tag, String corrVal) 
			: base($"{tag}:{corrVal}")
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

		public IDynamicObject Result { get; set; }
	}

	public class CallbackMessageResume : MessageBase<String>
	{
		public CallbackMessageResume(String tag, String corrVal) 
			: base($"{tag}:{corrVal}")
		{

		}

		public IDynamicObject Result { get; set; }
	}

	public class WaitApiCallbackSaga : SagaBaseDispatched<String, WaitCallbackMessage, CallbackMessage>
	{
		public WaitApiCallbackSaga() : base(nameof(WaitApiCallbackSaga))
		{

		}

		// serializable
		private String tag;
		private String correlationExpression;

		protected override Task Handle(IHandleContext context, WaitCallbackMessage message)
		{
			correlationExpression = message.CorrelationExpression;
			tag = message.Tag;
			CorrelationId.Value = message.CorrelationId.Value;
			return Task.CompletedTask;
		}

		protected override Task Handle(IHandleContext context, CallbackMessage message)
		{
			var cval = context.ScriptContext.GetValueFromObject<String>(message.Result, correlationExpression);

			context.SendMessage(new CallbackMessageResume(tag, cval)
			{
				Result = message.Result
			});
			return Task.CompletedTask;
		}
	}

	public class WaitApiCallbackProcessSaga : SagaBase<String>
	{
		public WaitApiCallbackProcessSaga() : base(nameof(WaitApiCallbackProcessSaga))
		{

		}

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
			if (context is null)
				throw new ArgumentNullException(nameof(context));

			_id = message.Id;
			CorrelationId.Value = message.CorrelationId.Value;
			return Task.CompletedTask;
		}

		public Task Handle(IHandleContext context, CallbackMessageResume message)
		{
			var resumeProcess = new ContinueActivityMessage(_id, null, message.Result);
			context.SendMessage(resumeProcess);
			IsComplete = true;
			return Task.CompletedTask;
		}
	}
}
