// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

using A2v10.ProcS.Infrastructure;
using Microsoft.Extensions.Configuration;

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

		public void LoadPlugins(String path, IConfiguration configuration)
		{
			List<(Assembly ass, ProcSPluginAttribute attr)> _assemblies = new List<(Assembly, ProcSPluginAttribute)>();

			foreach (var file in Directory.GetFiles(path, "*.dll"))
			{
				var name = Path.GetFileName(file).ToLowerInvariant();
				if (name.StartsWith("System.") || name.StartsWith("Microsoft."))
					continue;
				var assembly = Assembly.LoadFrom(file);
				var attr = assembly.GetCustomAttribute<ProcSPluginAttribute>();
				if (attr != null)
					_assemblies.Add((assembly, attr));
			}
			foreach (var ass1 in _assemblies)
				LoadPluginFromAssembly(ass1.ass, ass1.attr, configuration.GetSection(ass1.ass.GetName().Name));
		}

		public void LoadPluginFromAssembly(Assembly assembly, ProcSPluginAttribute attr, IConfiguration configuration)
		{
			var plugin = attr.CreatePlugin();
			plugin?.Init(serviceProvider, configuration);
			foreach (var probe in assembly.GetTypes())
			{
				var ISagaRegistrar = probe.GetInterface("ISagaRegistrar");
				if (ISagaRegistrar != null)
				{
					var prms = new List<Object>();
					var constr = probe.GetConstructors()[0];
					foreach (var param in constr.GetParameters())
					{
						var t = param.ParameterType;
						if (plugin != null && t.IsAssignableFrom(plugin.GetType()))
						{
							prms.Add(plugin);
						}
						else
						{
							prms.Add(serviceProvider.GetService(t));
						}
					}
					var registrar = Activator.CreateInstance(probe, prms.ToArray()) as ISagaRegistrar;
					registrar.Register(this);
				}
			}
		}
	}
}
