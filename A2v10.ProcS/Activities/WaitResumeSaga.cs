// Copyright ©️ 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Threading.Tasks;
using A2v10.ProcS.Infrastructure;

namespace A2v10.ProcS
{
	[ResourceKey(ukey)]
	public class WaitResumeMessage : MessageBase<String>
	{
		public const String ukey = ProcS.ResName + ":" + nameof(WaitResumeMessage);

		[RestoreWith]
		public WaitResumeMessage(Guid bookmark, Guid instance, String tag)
			: base($"{instance}:{tag}")
		{
			BookmarkId = bookmark;
			Tag = tag;
			InstanceId = instance;
		}

		public Guid BookmarkId { get; set; }

		public Guid InstanceId { get; set; }
		public String Tag { get; set; }

		public override void Store(IDynamicObject store, IResourceWrapper _)
		{
			store.Set("bookmark", BookmarkId);
			store.Set("instance", InstanceId);
			store.Set("tag", Tag);
		}

		public override void Restore(IDynamicObject store, IResourceWrapper _)
		{
			
		}
	}

	[ResourceKey(ukey)]
	public class ResumeMessage : MessageBase<String>
	{
		public const String ukey = ProcS.ResName + ":" + nameof(ResumeMessage);

		[RestoreWith]
		public ResumeMessage(Guid instance, String tag) : base($"{instance}:{tag}")
		{
			InstanceId = instance;
			Tag = tag;
		}

		public Guid InstanceId { get; set; }
		public String Tag { get; set; }
		public IDynamicObject Result { get; set; }

		public override void Store(IDynamicObject store, IResourceWrapper _)
		{
			store.Set("instance", InstanceId);
			store.Set("tag", Tag);
			store.Set(nameof(Result), Result);
		}

		public override void Restore(IDynamicObject store, IResourceWrapper _)
		{
			Result = store.GetDynamicObject(nameof(Result));
		}
	}

	public class WaitResumeSaga : SagaBaseDispatched<String, WaitResumeMessage, ResumeMessage>
	{
		public const String ukey = ProcS.ResName + ":" + nameof(WaitResumeSaga);

		public WaitResumeSaga() : base(ukey)
		{

		}

		// serializable
		private Guid bookmark;

		protected override Task Handle(IHandleContext context, WaitResumeMessage message)
		{
			bookmark = message.BookmarkId;
			SetCorrelation(message);
			return Task.CompletedTask;
		}

		protected override Task Handle(IHandleContext context, ResumeMessage message)
		{
			var msg = new ResumeBookmarkMessage(bookmark, message.Result);
			context.SendMessage(msg);
			return Task.CompletedTask;
		}

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
	}
}
