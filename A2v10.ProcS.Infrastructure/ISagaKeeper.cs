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
			Id = null;
			Saga = null;
			ServiceBusItem = null;
		}

		public PickedSaga(Guid? id, ISaga saga, IServiceBusItem item)
		{
			Available = true;
			Id = id;
			Saga = saga;
			ServiceBusItem = item;
		}

		public Boolean Available;
		public Guid? Id;
		public ISaga Saga;
		public IServiceBusItem ServiceBusItem;
	}

	public interface ISagaKeeper {
		Task SendMessage(IServiceBusItem item);
		Task<PickedSaga> PickSaga();
		Task ReleaseSaga(PickedSaga picked);
		Task FailSaga(PickedSaga picked, Exception exception);
	}
}
