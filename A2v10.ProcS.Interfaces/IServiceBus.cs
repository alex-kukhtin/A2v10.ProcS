// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;
using System.Threading.Tasks;

namespace A2v10.ProcS.Interfaces
{
	public interface ICorrelationId : IEquatable<ICorrelationId>
	{
		
	}

	public class CorrelationId<T> : ICorrelationId, IEquatable<CorrelationId<T>> where T : IEquatable<T>
	{
		public T Value { get; set; }

		/*public CorrelationId()
		{
			Value = default;
		}*/

		public CorrelationId(T value)
		{
			Value = value;
		}

		public Boolean Equals(ICorrelationId other)
		{
			if (other is CorrelationId<T> tt)
				return Equals(tt);
			return false;
		}

		public override Int32 GetHashCode()
		{
			return Value?.GetHashCode() ?? 0;
		}

		public Boolean Equals(CorrelationId<T> other)
		{
			if (Value == null) return other.Value == null;
			return Value.Equals(other.Value);
		}
	}

	public interface IMessage
	{
		ICorrelationId CorrelationId { get; }
	}

	public class MessageBase<CorrelationT> : IMessage where CorrelationT : IEquatable<CorrelationT>
	{
		public MessageBase(CorrelationT correlationId)
		{
			CorrelationId = new CorrelationId<CorrelationT>(correlationId);
		}

		public CorrelationId<CorrelationT> CorrelationId { get; }

		ICorrelationId IMessage.CorrelationId => CorrelationId;
	}

	public interface ISaga
	{
		Boolean IsComplete { get; }
		ICorrelationId CorrelationId { get; }
		Task Handle(IHandleContext context, IMessage message);
	}

	public abstract class SagaBase<CorrelationT> : ISaga where CorrelationT : IEquatable<CorrelationT>
	{
		public CorrelationId<CorrelationT> CorrelationId { get; } = new CorrelationId<CorrelationT>(default);

		ICorrelationId ISaga.CorrelationId => CorrelationId;

		public Boolean IsComplete { get; set; }
		public abstract Task Handle(IHandleContext context, IMessage message);
	}

	public abstract class SagaBaseDispatched<CorrelationT, MessageT1> : SagaBase<CorrelationT>
		where CorrelationT : IEquatable<CorrelationT>
		where MessageT1 : IMessage
	{
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

	public interface IServiceBus
	{
		void Send(IMessage message);
		Task Run();
	}
}
