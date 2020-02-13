// Copyright © 2020 Alex Kukhtin. All rights reserved.

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

		private readonly Object _lck = new Object();
		
		protected ISagaFactory GetFactory(Type type)
		{
			lock (_lck)
			{
				InitPlugins();
			}
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

		protected class InternalPlugin
		{
			private readonly Boolean valid;
			private Boolean init;
			private readonly Assembly ass;
			private readonly ProcSPluginAttribute attr;
			private readonly IConfiguration conf;

			public Boolean IsValid => valid;

			public InternalPlugin(String file, IConfiguration config)
			{
				init = false;
				ass = Assembly.LoadFrom(file);
				attr = ass.GetCustomAttribute<ProcSPluginAttribute>();
				valid = attr != null;
				conf = config.GetSection(ass.GetName().Name);
			}

			public void Init(ISagaManager mgr, IServiceProvider serviceProvider)
			{
				if (!valid) throw new Exception($"Assembly {ass.FullName} does not contain Valid ProcS plugin");
				if (init) return;
				var plugin = attr.CreatePlugin();
				plugin?.Init(serviceProvider, conf);
				foreach (var probe in ass.GetTypes())
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
						registrar.Register(mgr);
					}
				}
				init = true;
			}
		}

		protected Dictionary<String, InternalPlugin> _plugs = new Dictionary<String, InternalPlugin>();

		public void LoadPlugins(String path, IConfiguration configuration)
		{
			foreach (var file in Directory.GetFiles(path, "*.dll"))
			{
				var name = Path.GetFileName(file).ToLowerInvariant();
				if (name.StartsWith("system.") || name.StartsWith("microsoft."))
					continue;
				_plugs.Add(file, new InternalPlugin(file, configuration));
			}
		}

		protected void InitPlugins()
		{
			foreach (var p in _plugs.Values)
			{
				if (!p.IsValid) continue;
				p.Init(this, serviceProvider);
			}
		}
	}
}
