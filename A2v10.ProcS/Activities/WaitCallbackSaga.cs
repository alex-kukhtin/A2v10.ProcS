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

		public override void Store(IDynamicObject store, IResourceWrapper _)
		{
			store.Set("tag", Tag); // !!! as constructor parameter name !!!
			store.Set(nameof(CorrelationExpression), CorrelationExpression);
		}

		public override void Restore(IDynamicObject store, IResourceWrapper _)
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

		public override void Store(IDynamicObject store, IResourceWrapper _)
		{
			store.Set("tag", Tag); // !!! as constructor parameter name !!!
			store.Set(nameof(Result), Result);
		}

		public override void Restore(IDynamicObject store, IResourceWrapper _)
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

		public override void Store(IDynamicObject store, IResourceWrapper _)
		{
			store.Set("bookmark", BookmarkId); // as ctor parameter name
			store.Set("tag", Tag);
			store.Set("corrVal", CorrelationValue);
		}

		public override void Restore(IDynamicObject store, IResourceWrapper _)
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
		public CorrelatedCallbackMessage(String tag, String corrId)
			: base($"{tag}:{corrId}")
		{
			Tag = tag;
			CorrId = corrId;
		}

		public String Tag;
		public String CorrId;
		public IDynamicObject Result { get; set; }

		public override void Store(IDynamicObject store, IResourceWrapper _)
		{
			store.Set("tag", Tag); // !!! as constructor parameter name !!!
			store.Set("corrId", CorrId); // !!! as constructor parameter name !!!
			store.Set(nameof(Result), Result);
		}

		public override void Restore(IDynamicObject store, IResourceWrapper _)
		{
			//Tag = store.Get<String>("tag");
			//CorrId = store.Get<String>("corrId");
			Result = store.GetDynamicObject(nameof(Result));
		}
	}

	public class RegisterCallbackSaga : SagaBaseDispatched<String, RegisterCallbackMessage, CallbackMessage>
	{
		public const String ukey = ProcS.ResName + ":" + nameof(RegisterCallbackSaga);

		protected readonly IScriptEngine _scriptEngine;

		public RegisterCallbackSaga(IScriptEngine scriptEngine) : base(ukey)
		{
			_scriptEngine = scriptEngine;
			isWaiting = false;
		}

		// serializable
		private Boolean isWaiting;
		private String tag;
		private String correlationExpression;

		protected override Task Handle(IHandleContext context, RegisterCallbackMessage message)
		{
			correlationExpression = message.CorrelationExpression;
			tag = message.Tag;
			isWaiting = true;
			SetCorrelation(message);
			return Task.CompletedTask;
		}

		protected override Task Handle(IHandleContext context, CallbackMessage message)
		{
			if (!isWaiting)
				return Task.CompletedTask;

			string cval;
			using (var sc = _scriptEngine.CreateContext())
			{
				cval = sc.GetValueFromObject<String>(message.Result, correlationExpression);
			}

			context.SendMessage(new CorrelatedCallbackMessage(tag, cval)
			{
				Result = message.Result
			});
			return Task.CompletedTask;
		}

		public override IDynamicObject Store(IResourceWrapper _)
		{
			var d = new DynamicObject();
			d.Set(nameof(isWaiting), isWaiting);
			d.Set(nameof(tag), tag);
			d.Set(nameof(correlationExpression), correlationExpression);
			return d;
		}

		public override void Restore(IDynamicObject store, IResourceWrapper _)
		{
			isWaiting = store.Get<Boolean>(nameof(isWaiting));
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

		public override IDynamicObject Store(IResourceWrapper _)
		{
			var d = new DynamicObject();
			d.Set(nameof(bookmark), bookmark);
			return d;
		}

		public override void Restore(IDynamicObject store, IResourceWrapper _)
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
			var msg = new ResumeBookmarkMessage(bookmark, message.Result);
			context.SendMessage(msg);
			IsComplete = true;
			return Task.CompletedTask;
		}
	}
}
