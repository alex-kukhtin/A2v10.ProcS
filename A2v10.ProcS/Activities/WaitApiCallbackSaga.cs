// Copyright ©️ 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Net.Http;
using System.Threading.Tasks;
using A2v10.ProcS.Infrastructure;

namespace A2v10.ProcS
{

	public class RegisterCallbackMessage : MessageBase<String>
	{
		public RegisterCallbackMessage(String tag) : base(tag)
		{
			Tag = tag;
		}

		public String Tag { get; set; }
		public String CorrelationExpression { get; set; }
	}

	public class CallbackMessage : MessageBase<String>
	{
		public CallbackMessage(String tag) : base(tag)
		{
			
		}

		public IDynamicObject Result { get; set; }
	}

	public class WaitCallbackMessage : MessageBase<String>
	{
		public WaitCallbackMessage(Guid bookmark, String tag, String corrVal)
			: base($"{tag}:{corrVal}")
		{
			BookmarkId = bookmark;
			Tag = tag;
			CorrelationValue = corrVal;
		}

		public Guid BookmarkId { get; set; }

		public String Tag { get; set; }
		public String CorrelationValue { get; set; }
	}

	public class CorrelatedCallbackMessage : MessageBase<String>
	{
		public CorrelatedCallbackMessage(String tag, String corrVal) 
			: base($"{tag}:{corrVal}")
		{

		}

		public IDynamicObject Result { get; set; }
	}

	public class RegisterCallbackSaga : SagaBaseDispatched<String, RegisterCallbackMessage, CallbackMessage>
	{
		public RegisterCallbackSaga() : base(nameof(RegisterCallbackSaga))
		{

		}

		// serializable
		private String tag;
		private String correlationExpression;

		protected override Task Handle(IHandleContext context, RegisterCallbackMessage message)
		{
			correlationExpression = message.CorrelationExpression;
			tag = message.Tag;
			SetCorrelation(message);
			return Task.CompletedTask;
		}

		protected override Task Handle(IHandleContext context, CallbackMessage message)
		{
			var cval = context.ScriptContext.GetValueFromObject<String>(message.Result, correlationExpression);

			context.SendMessage(new CorrelatedCallbackMessage(tag, cval)
			{
				Result = message.Result
			});
			return Task.CompletedTask;
		}
	}

	public class CallbackCorrelationSaga : SagaBaseDispatched<String, WaitCallbackMessage, CorrelatedCallbackMessage>
	{
		public CallbackCorrelationSaga() : base(nameof(CallbackCorrelationSaga))
		{

		}

		// serializable
		private Guid bookmark;

		protected override Task Handle(IHandleContext context, WaitCallbackMessage message)
		{
			bookmark = message.BookmarkId;
			SetCorrelation(message);
			return Task.CompletedTask;
		}

		protected override Task Handle(IHandleContext context, CorrelatedCallbackMessage message)
		{
			context.ResumeBookmark(bookmark, message.Result);
			IsComplete = true;
			return Task.CompletedTask;
		}
	}
}
