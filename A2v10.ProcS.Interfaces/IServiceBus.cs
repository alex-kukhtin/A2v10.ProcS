// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;
using System.Threading.Tasks;

namespace A2v10.ProcS.Interfaces
{
	public interface IMessage
	{
		String CorrelationId { get; }
	}

	public interface ISaga
	{
		Boolean IsComplete { get; }

		Task<String> Handle(IMessage message);
	}

	public interface IServiceBus
	{
		void Send(IMessage message);
		Task Run();
	}
}
