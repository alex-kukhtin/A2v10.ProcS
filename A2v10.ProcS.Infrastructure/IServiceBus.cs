// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace A2v10.ProcS.Infrastructure
{
	public interface ICorrelationId : IEquatable<ICorrelationId>
	{
		
	}

	public interface IMessage
	{
		ICorrelationId CorrelationId { get; }
	}

	public interface ISaga
	{
		String Kind { get; }
		Boolean IsComplete { get; }
		ICorrelationId CorrelationId { get; }
		Task Handle(IHandleContext context, IMessage message);
	}

	public interface IServiceBus
	{
		void Send(IMessage message);
		void Send(IEnumerable<IMessage> messages);
		void SendSequence(IEnumerable<IMessage> messages);
		Task Run(CancellationToken token);
	}
}
