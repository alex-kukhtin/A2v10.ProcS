// Copyright ©️ 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Threading.Tasks;
using A2v10.ProcS.Infrastructure;

namespace A2v10.ProcS
{
	[ResourceKey(ukey)]
	public class WaitBookmarkResumeMessage : MessageBase<String>
	{
		public const String ukey = ProcS.ResName + ":" + nameof(WaitBookmarkResumeMessage);

		[RestoreWith]
		public WaitBookmarkResumeMessage(Guid bookmark, Guid instance, String tag)
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
	public class BookmarkResumeMessage : MessageBase<String>
	{
		public const String ukey = ProcS.ResName + ":" + nameof(BookmarkResumeMessage);

		[RestoreWith]
		public BookmarkResumeMessage(Guid instance, String tag) : base($"{instance}:{tag}")
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

	public class SetBookmarkSaga : SagaBaseDispatched<String, WaitBookmarkResumeMessage, BookmarkResumeMessage>
	{
		public const String ukey = ProcS.ResName + ":" + nameof(SetBookmarkSaga);

		public SetBookmarkSaga() : base(ukey)
		{

		}

		// serializable
		private Guid bookmark;

		protected override Task Handle(IHandleContext context, WaitBookmarkResumeMessage message)
		{
			bookmark = message.BookmarkId;
			SetCorrelation(message);
			return Task.CompletedTask;
		}

		protected override Task Handle(IHandleContext context, BookmarkResumeMessage message)
		{
			context.ResumeBookmark(bookmark, message.Result);
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
