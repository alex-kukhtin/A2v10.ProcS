// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Threading.Tasks;

namespace A2v10.ProcS.Infrastructure
{
	public interface ISagaKeeperKey : IEquatable<ISagaKeeperKey>
	{
		String SagaKind { get; }
		ICorrelationId CorrelationId { get; }
	}

	public interface ISagaKeeper {
		ISaga GetSagaForMessage(IMessage message, out ISagaKeeperKey key, out bool isNew);
		void SagaUpdate(ISaga saga, ISagaKeeperKey key);
	}
}
