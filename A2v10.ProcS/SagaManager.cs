// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using A2v10.ProcS.Infrastructure;
using Microsoft.Extensions.Configuration;

namespace A2v10.ProcS
{
	
	public class SagaManager : ISagaManager
	{
		protected readonly Dictionary<String, ISagaFactory> _messagesMap = new Dictionary<String, ISagaFactory>();
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
				var att = t.GetCustomAttribute<ResourceKeyAttribute>();
				if (att == null) throw new Exception("Registred message type must have ResourceKeyAttribute");
				_messagesMap.Add(att.Key, factory);
			}
		}

		public ISagaResolver Resolver => new InternalResolver(_messagesMap);

		protected class InternalResolver : ISagaResolver
		{
			private readonly IDictionary<String, ISagaFactory> map;

			public InternalResolver(IDictionary<String, ISagaFactory> map)
			{
				this.map = map;
			}

			public IEnumerable<KeyValuePair<String, ISagaFactory>> GetMap()
			{
				return map;
			}

			public ISagaFactory GetSagaFactory(IMessage message)
			{
				return GetSagaFactory(message.GetType());
			}

			public ISagaFactory GetSagaFactory(Type messageType)
			{
				var att = messageType.GetCustomAttribute<ResourceKeyAttribute>();
				return map[att.Key];
			}

			public ISagaFactory GetSagaFactory<TMessage>() where TMessage : IMessage
			{
				return GetSagaFactory(typeof(TMessage));
			}
		}
	}
}
