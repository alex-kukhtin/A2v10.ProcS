// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace A2v10.ProcS.Infrastructure
{
	public interface ISagaRegistrar
	{
		void Register(IResourceManager rmgr, ISagaManager smgr);
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

		ISagaResolver Resolver { get; }
	}

	public interface ISagaResolver
	{
		ISagaFactory GetSagaFactory(IMessage message);
		ISagaFactory GetSagaFactory(Type messageType);
		ISagaFactory GetSagaFactory<TMessage>() where TMessage : IMessage;
		IEnumerable<KeyValuePair<String, ISagaFactory>> GetMap();
	}
}
