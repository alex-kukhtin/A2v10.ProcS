// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;
using System.Threading.Tasks;

namespace A2v10.ProcS.Infrastructure
{
	public abstract class SagaBase<CorrelationT> : ISaga where CorrelationT : IEquatable<CorrelationT>
	{
		protected SagaBase(String kind)
		{
			Kind = kind;
		}
		public String Kind { get; private set; }

		public CorrelationId<CorrelationT> CorrelationId { get; } = new CorrelationId<CorrelationT>(default);

		ICorrelationId ISaga.CorrelationId => CorrelationId;

		public Boolean IsComplete { get; set; }
		public abstract Task Handle(IHandleContext context, IMessage message);
	}

	public abstract class SagaBaseDispatched<CorrelationT, MessageT1> : SagaBase<CorrelationT>
		where CorrelationT : IEquatable<CorrelationT>
		where MessageT1 : IMessage
	{
		protected SagaBaseDispatched(String kind) : base(kind)
		{

		}

		public async override Task Handle(IHandleContext context, IMessage message)
		{
			switch (message)
			{
				case MessageT1 msg:
					await Handle(context, msg);
					break;
				default:
					throw new ArgumentOutOfRangeException(message.GetType().FullName);
			}
		}

		protected abstract Task Handle(IHandleContext context, MessageT1 message);
	}

	public abstract class SagaBaseDispatched<CorrelationT, MessageT1, MessageT2> : SagaBase<CorrelationT>
		where CorrelationT : IEquatable<CorrelationT>
		where MessageT1 : IMessage
		where MessageT2 : IMessage
	{
		protected SagaBaseDispatched(String kind) : base(kind)
		{

		}

		public async override Task Handle(IHandleContext context, IMessage message)
		{
			switch (message)
			{
				case MessageT1 msg:
					await Handle(context, msg);
					break;
				case MessageT2 msg:
					await Handle(context, msg);
					break;
				default:
					throw new ArgumentOutOfRangeException(message.GetType().FullName);
			}
		}

		protected abstract Task Handle(IHandleContext context, MessageT1 message);
		protected abstract Task Handle(IHandleContext context, MessageT2 message);
	}

	public abstract class SagaBaseDispatched<CorrelationT, MessageT1, MessageT2, MessageT3> : SagaBase<CorrelationT>
		where CorrelationT : IEquatable<CorrelationT>
		where MessageT1 : IMessage
		where MessageT2 : IMessage
		where MessageT3 : IMessage
	{
		protected SagaBaseDispatched(String kind) : base(kind)
		{

		}

		public async override Task Handle(IHandleContext context, IMessage message)
		{
			switch (message)
			{
				case MessageT1 msg:
					await Handle(context, msg);
					break;
				case MessageT2 msg:
					await Handle(context, msg);
					break;
				case MessageT3 msg:
					await Handle(context, msg);
					break;
				default:
					throw new ArgumentOutOfRangeException(message.GetType().FullName);
			}
		}

		protected abstract Task Handle(IHandleContext context, MessageT1 message);
		protected abstract Task Handle(IHandleContext context, MessageT2 message);
		protected abstract Task Handle(IHandleContext context, MessageT3 message);
	}

	public abstract class SagaBaseDispatched<CorrelationT, MessageT1, MessageT2, MessageT3, MessageT4> : SagaBase<CorrelationT>
		where CorrelationT : IEquatable<CorrelationT>
		where MessageT1 : IMessage
		where MessageT2 : IMessage
		where MessageT3 : IMessage
		where MessageT4 : IMessage
	{
		protected SagaBaseDispatched(String kind) : base(kind)
		{

		}

		public async override Task Handle(IHandleContext context, IMessage message)
		{
			switch (message)
			{
				case MessageT1 msg:
					await Handle(context, msg);
					break;
				case MessageT2 msg:
					await Handle(context, msg);
					break;
				case MessageT3 msg:
					await Handle(context, msg);
					break;
				case MessageT4 msg:
					await Handle(context, msg);
					break;
				default:
					throw new ArgumentOutOfRangeException(message.GetType().FullName);
			}
		}

		protected abstract Task Handle(IHandleContext context, MessageT1 message);
		protected abstract Task Handle(IHandleContext context, MessageT2 message);
		protected abstract Task Handle(IHandleContext context, MessageT3 message);
		protected abstract Task Handle(IHandleContext context, MessageT4 message);
	}
}
