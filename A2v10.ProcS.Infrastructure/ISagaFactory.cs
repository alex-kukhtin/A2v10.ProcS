// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;
using System.Threading.Tasks;

namespace A2v10.ProcS.Infrastructure
{
	public interface ISagaFactory
	{
		ISaga CreateSaga();
		String SagaKind { get; }
	}

	public abstract class SagaFactoryBase : ISagaFactory
	{
		protected SagaFactoryBase(String kind)
		{
			SagaKind = kind;
		}

		public String SagaKind { get; private set; }

		protected abstract ISaga CreateSagaInternal();

		public ISaga CreateSaga()
		{
			var saga = CreateSagaInternal();
			if (saga.Kind != SagaKind) throw new Exception("SagaFactory created a Saga of a wrong Kind");
			return saga;
		}
	}

	public class DelegateSagaFactory : SagaFactoryBase
	{
		private readonly Func<ISaga> deleg;

		public DelegateSagaFactory(String kind, Func<ISaga> deleg) : base(kind)
		{
			this.deleg = deleg;
		}

		protected override ISaga CreateSagaInternal()
		{
			return deleg();
		}
	}

	public class ConstructSagaFactory<SagaT> : SagaFactoryBase where SagaT : ISaga, new()
	{
		public ConstructSagaFactory(String kind) : base(kind)
		{

		}

		protected override ISaga CreateSagaInternal()
		{
			return new SagaT();
		}
	}


}
