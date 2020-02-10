// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

using A2v10.ProcS.Infrastructure;

namespace A2v10.ProcS
{
	public class SagaManager : ISagaManager
	{
		private readonly Dictionary<Type, ISagaFactory> _messagesMap = new Dictionary<Type, ISagaFactory>();
		private readonly IServiceProvider serviceProvider;

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

		public ISagaFactory GetSagaFactory(IMessage message)
		{
			return GetSagaFactory(message.GetType());
		}

		public ISagaFactory GetSagaFactory(Type messageType)
		{
			return _messagesMap[messageType];
		}

		public ISagaFactory GetSagaFactory<TMessage>() where TMessage : IMessage
		{
			return GetSagaFactory(typeof(TMessage));
		}

		public void LoadPlugins(String path)
		{
			List<Assembly> _assemblies = new List<Assembly>();

			foreach (var file in Directory.GetFiles(path, "*.dll"))
			{
				var name = Path.GetFileName(file).ToLowerInvariant();
				if (name.StartsWith("System.") || name.StartsWith("Microsoft."))
					continue;
				var assembly = Assembly.LoadFrom(file);
				var attr = assembly.GetCustomAttribute<ProcSPluginAttribute>();
				if (attr != null)
					_assemblies.Add(assembly);
			}
			foreach (var ass1 in _assemblies)
				LoadPluginFromAssembly(ass1);
		}

		void LoadPluginFromAssembly(Assembly assembly)
		{
			foreach (var probe in assembly.GetTypes())
			{
				var ISagaRegistrar = probe.GetInterface("ISagaRegistrar");
				if (ISagaRegistrar != null)
				{
					var registrar = Activator.CreateInstance(probe) as ISagaRegistrar;
					registrar.Register(this, serviceProvider);
				}
			}
		}
	}
}
