// Copyright ©️ 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Threading.Tasks;
using A2v10.ProcS.Infrastructure;

namespace A2v10.ProcS
{
	[ResourceKey(ukey)]
	public class RegisterCallbackMessage : MessageBase<String>
	{
		public const String ukey = ProcS.ResName + ":" + nameof(RegisterCallbackMessage);

		[RestoreWith]
		public RegisterCallbackMessage(String tag) : base(tag)
		{
			Tag = tag;
		}

		public String Tag { get; set; }
		public String CorrelationExpression { get; set; }

		public override void Store(IDynamicObject store)
		{
			store.Set("tag", Tag); // !!! as constructor parameter name !!!
			store.Set(nameof(CorrelationExpression), CorrelationExpression);
		}

		public override void Restore(IDynamicObject store)
		{
			Tag = store.Get<String>("tag");
			CorrelationExpression = store.Get<String>(nameof(CorrelationExpression));
		}
	}

	[ResourceKey(ukey)]
	public class CallbackMessage : MessageBase<String>
	{
		public const String ukey = ProcS.ResName + ":" + nameof(CallbackMessage);

		[RestoreWith]
		public CallbackMessage(String tag) : base(tag)
		{
			Tag = tag;
		}

		public String Tag { get; set; }
		public IDynamicObject Result { get; set; }

		public override void Store(IDynamicObject store)
		{
			store.Set("tag", Tag); // !!! as constructor parameter name !!!
			store.Set(nameof(Result), Result);
		}

		public override void Restore(IDynamicObject store)
		{
			Tag = store.Get<String>("tag");
			Result = store.GetDynamicObject(nameof(Result));
		}
	}

	[ResourceKey(ukey)]
	public class WaitCallbackMessage : MessageBase<String>
	{
		public const String ukey = ProcS.ResName + ":" + nameof(WaitCallbackMessage);

		[RestoreWith]
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

		public override void Store(IDynamicObject store)
		{
			store.Set("bookmark", BookmarkId); // as ctor parameter name
			store.Set("tag", Tag);
			store.Set("corrVal", CorrelationValue);
		}

		public override void Restore(IDynamicObject store)
		{
			BookmarkId = store.Get<Guid>("bookmark");
			Tag = store.Get<String>("tag");
			CorrelationValue = store.Get<String>("corrVal");
		}
	}

	[ResourceKey(ukey)]
	public class CorrelatedCallbackMessage : MessageBase<String>
	{
		public const String ukey = ProcS.ResName + ":" + nameof(CorrelatedCallbackMessage);

		[RestoreWith]
		public CorrelatedCallbackMessage(String tag, String correlationId) 
			: base(CorrelatedCallbackMessage.getBaseCorrelationId(tag, correlationId))
		{
			Tag = tag;
		}

		static String getBaseCorrelationId(String tag, String correlationId)
		{
			if (correlationId.StartsWith(tag))
				return correlationId;
			return $"{tag}:{correlationId}";
		}

		public String Tag;
		public IDynamicObject Result { get; set; }

		public override void Store(IDynamicObject store)
		{
			store.Set("tag", Tag); // !!! as constructor parameter name !!!
			store.Set(nameof(Result), Result);
		}

		public override void Restore(IDynamicObject store)
		{
			Tag = store.Get<String>("tag");
			Result = store.GetDynamicObject(nameof(Result));
		}
	}

	public class RegisterCallbackSaga : SagaBaseDispatched<String, RegisterCallbackMessage, CallbackMessage>
	{
		public const String ukey = ProcS.ResName + ":" + nameof(RegisterCallbackSaga);

		public RegisterCallbackSaga() : base(ukey)
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

		public override IDynamicObject Store()
		{
			var d = new DynamicObject();
			d.Set(nameof(tag), tag);
			d.Set(nameof(correlationExpression), correlationExpression);
			return d;
		}

		public override void Restore(IDynamicObject store)
		{
			tag = store.Get<String>(nameof(tag));
			correlationExpression = store.Get<String>(nameof(correlationExpression));
		}
	}

	public class CallbackCorrelationSaga : SagaBaseDispatched<String, WaitCallbackMessage, CorrelatedCallbackMessage>
	{
		public const String ukey = ProcS.ResName + ":" + nameof(CallbackCorrelationSaga);

		public CallbackCorrelationSaga() : base(ukey)
		{
		}

		// serializable
		private Guid bookmark;

		public override IDynamicObject Store()
		{
			var d = new DynamicObject();
			d.Set(nameof(bookmark), bookmark);
			return d;
		}

		public override void Restore(IDynamicObject store)
		{
			bookmark = store.Get<Guid>(nameof(bookmark));
		}

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
