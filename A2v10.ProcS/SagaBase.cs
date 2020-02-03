// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;
using System.Threading.Tasks;

using A2v10.ProcS.Interfaces;

namespace A2v10.ProcS
{
	public abstract class SagaBase : ISaga
	{
		public Boolean IsComplete { get; set; }

		public Guid Id { get; protected set; }

		private readonly IServiceBus _serviceBus;
		private readonly IInstanceStorage _instanceStorage;

		/* required */
		public SagaBase(Guid id, IServiceBus serviceBus, IInstanceStorage instanceStorage)
		{
			Id = id;
			_serviceBus = serviceBus;
			_instanceStorage = instanceStorage;
		}

		public IServiceBus ServiceBus => _serviceBus;
		public IInstanceStorage InstanceStorage => _instanceStorage;

		public abstract Task<String> Handle(IMessage message);
	}
}
