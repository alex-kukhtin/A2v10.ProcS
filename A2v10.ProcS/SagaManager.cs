// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using A2v10.ProcS.Infrastructure;
using Microsoft.Extensions.Configuration;

namespace A2v10.ProcS
{
	
	public class SagaManager : ISagaManager
	{
		protected readonly Dictionary<Type, ISagaFactory> _messagesMap = new Dictionary<Type, ISagaFactory>();
		protected readonly IServiceProvider serviceProvider;

		public SagaManager(IServiceProvider serviceProvider)
		{
			this.serviceProvider = serviceProvider;
		}

		public void RegisterSagaFactory<TMessage>(ISagaFactory factory) where TMessage : IMessage
		{
			RegisterSagaFactory(factory, typeof(TMessage));
		}

		public void RegisterSagaFactory<TMessage1, TMessage2>(ISagaFactory factory)
			where TMessage1 : IMessage
			where TMessage2 : IMessage
		{
			RegisterSagaFactory(factory, typeof(TMessage1), typeof(TMessage2));
		}

		public void RegisterSagaFactory<TMessage1, TMessage2, TMessage3>(ISagaFactory factory)
			where TMessage1 : IMessage
			where TMessage2 : IMessage
			where TMessage3 : IMessage
		{
			RegisterSagaFactory(factory, typeof(TMessage1), typeof(TMessage2), typeof(TMessage3));
		}

		public void RegisterSagaFactory<TMessage1, TMessage2, TMessage3, TMessage4>(ISagaFactory factory)
			where TMessage1 : IMessage
			where TMessage2 : IMessage
			where TMessage3 : IMessage
			where TMessage4 : IMessage
		{
			RegisterSagaFactory(factory, typeof(TMessage1), typeof(TMessage2), typeof(TMessage3), typeof(TMessage4));
		}

		public void RegisterSagaFactory(ISagaFactory factory, params Type[] types)
		{
			RegisterSagaFactory(factory, types as IEnumerable<Type>);
		}

		public void RegisterSagaFactory(ISagaFactory factory, IEnumerable<Type> types)
		{
			foreach (var t in types)
			{
				_messagesMap.Add(t, factory);
			}
		}

		protected ISagaFactory GetFactory(Type type)
		{
			return _messagesMap[type];
		}

		public ISagaResolver Resolver => new InternalResolver(GetFactory);

		protected class InternalResolver : ISagaResolver
		{
			private readonly Func<Type, ISagaFactory> map;

			public InternalResolver(Func<Type, ISagaFactory> map)
			{
				this.map = map;
			}

			public ISagaFactory GetSagaFactory(IMessage message)
			{
				return GetSagaFactory(message.GetType());
			}

			public ISagaFactory GetSagaFactory(Type messageType)
			{
				return map(messageType);
			}

			public ISagaFactory GetSagaFactory<TMessage>() where TMessage : IMessage
			{
				return GetSagaFactory(typeof(TMessage));
			}
		}
	}
}
