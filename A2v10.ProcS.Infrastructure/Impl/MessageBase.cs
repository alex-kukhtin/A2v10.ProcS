// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;

namespace A2v10.ProcS.Infrastructure
{
	public abstract class MessageBase<CorrelationT> : IMessage where CorrelationT : IEquatable<CorrelationT>
	{
		public MessageBase(CorrelationT correlationId)
		{
			CorrelationId = new CorrelationId<CorrelationT>(correlationId);
		}
		
		public CorrelationId<CorrelationT> CorrelationId { get; }

		ICorrelationId IMessage.CorrelationId => CorrelationId;

		public virtual void Restore(IDynamicObject store)
		{

		}

		public virtual void Store(IDynamicObject storage)
		{

		}

		public IDynamicObject Store()
		{
			var obj = new DynamicObject();
			obj.Set("correlationId", CorrelationId.Value);
			Store(obj);
			return obj;
		}
	}
}
