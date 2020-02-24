// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace A2v10.ProcS.Infrastructure
{
	public interface ISagaKeeperKey : IEquatable<ISagaKeeperKey>
	{
		String SagaKind { get; }
		ICorrelationId CorrelationId { get; }
	}

	public struct PickedSaga
	{
		public PickedSaga(Boolean avail)
		{
			Available = avail;
			Key = null;
			Saga = null;
			ServiceBusItem = null;
		}

		public PickedSaga(ISagaKeeperKey key, ISaga saga, IServiceBusItem item)
		{
			Available = true;
			Key = key;
			Saga = saga;
			ServiceBusItem = item;
		}

		public Boolean Available;
		public ISagaKeeperKey Key;
		public ISaga Saga;
		public IServiceBusItem ServiceBusItem;
	}

	public interface ISagaKeeper {
		Task SendMessage(IServiceBusItem item);
		Task<PickedSaga> PickSaga();
		Task ReleaseSaga(PickedSaga picked);
	}
}
