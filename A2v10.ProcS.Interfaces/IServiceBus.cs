// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;
using System.Threading.Tasks;

namespace A2v10.ProcS.Interfaces
{
	public interface IMessage
	{
		Guid Id { get; }
	}

	public interface IDomainEvent : IMessage
	{
	}

	public interface ISaga
	{
		Boolean IsComplete { get; }
		Guid Id { get; }

		Task Start(IMessage message);
		Task Handle(IMessage message);
	}

	public interface IServiceBus
	{
		void Send(IMessage message);
		Task Run();
	}
}
