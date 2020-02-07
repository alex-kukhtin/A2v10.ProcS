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
		void RegisterSagaFactory<TMessage>(ISagaFactory factory)
			where TMessage : IMessage;
		void RegisterSagaFactory<TMessage1, TMessage2>(ISagaFactory factory)
			where TMessage1 : IMessage
			where TMessage2 : IMessage;
		void RegisterSagaFactory<TMessage1, TMessage2, TMessage3>(ISagaFactory factory)
			where TMessage1 : IMessage
			where TMessage2 : IMessage
			where TMessage3 : IMessage;
		void RegisterSagaFactory<TMessage1, TMessage2, TMessage3, TMessage4>(ISagaFactory factory)
			where TMessage1 : IMessage
			where TMessage2 : IMessage
			where TMessage3 : IMessage
			where TMessage4 : IMessage;
		void RegisterSagaFactory(ISagaFactory factory, params Type[] types);
		void RegisterSagaFactory(ISagaFactory factory, IEnumerable<Type> types);
	}
}
