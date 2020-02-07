using System;
using System.Collections.Generic;
using System.Text;

namespace A2v10.ProcS.Infrastructure
{
	public interface ISagaRegistrar
	{
		void Register(ISagaManager mgr, IServiceProvider provider);
	}

	public interface ISagaManager
	{
		void RegisterSagaFactory<TMessage>(ISagaFactory factory) where TMessage : IMessage;
	}
}
