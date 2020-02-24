// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using A2v10.ProcS.Infrastructure;
using Microsoft.Extensions.Configuration;

namespace A2v10.ProcS
{
	public class PluginManager : IPluginManager
	{
		private readonly IServiceProvider serviceProvider;


		public PluginManager(IServiceProvider serviceProvider)
		{
			this.serviceProvider = serviceProvider;
		}

		protected class InternalPlugin
		{
			private readonly Boolean valid;
			private Boolean init;
			private IPlugin plugin;
			private readonly Assembly ass;
			private readonly ProcSPluginAttribute attr;
			private readonly IConfiguration conf;

			public Boolean IsValid => valid;

			public String Name => attr.Name;

			public Boolean IsFromCurrentAssembly()
			{
				return ass == Assembly.GetExecutingAssembly();
			}

			public T GetPlugin<T>() where T : class, IPlugin
			{
				if (!valid) throw new Exception($"Assembly {ass.FullName} does not contain Valid ProcS plugin");
				if (!init) throw new Exception($"Plugin {attr.Name} is not Initialized");
				if (attr.PluginType == null) throw new Exception($"Plugin {attr.Name} does not have Plugin class");
				if (!typeof(T).IsAssignableFrom(attr.PluginType)) throw new Exception($"Plugin {attr.Name} class type {attr.PluginType} is not assignable to {typeof(T)}");
				return plugin as T;
			}

			public InternalPlugin(String file, IConfiguration config)
			{
				init = false;
				ass = Assembly.LoadFrom(file);
				attr = ass.GetCustomAttribute<ProcSPluginAttribute>();
				valid = attr != null;
				conf = config.GetSection(ass.GetName().Name);
			}

			public void RegisterResources(IResourceManager rmgr, ISagaManager smgr, IServiceProvider serviceProvider)
			{
				foreach (var probe in ass.GetTypes())
				{
					var IActivity = probe.GetInterface("IActivity");
                    if (IActivity != null)
                    {
						var att = probe.GetCustomAttribute<ResourceKeyAttribute>();
						if (att == null) continue;
						rmgr.RegisterResourceFactory(att.Key, new TypeResourceFactory(probe));
					}
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
						registrar.Register(rmgr, smgr);
					}
				}
			}

			public void Init(IServiceProvider serviceProvider)
			{
				if (!valid) throw new Exception($"Assembly {ass.FullName} does not contain Valid ProcS plugin");
				if (init) return;
				plugin = attr.CreatePlugin();
				plugin?.Init(serviceProvider, conf);
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
				p.Init(serviceProvider);
			}
		}

		public void RegisterResources(IResourceManager rmgr, ISagaManager smgr)
		{
			lock (this) {
				InitPlugins();
			}
			foreach (var p in _plugs.Values)
			{
				if (!p.IsValid) continue;
				p.RegisterResources(rmgr, smgr, serviceProvider);
			}
		}

		public T GetCurrentPlugin<T>() where T : class, IPlugin
		{
			foreach (var pl in _plugs.Values)
			{
				if (pl.IsFromCurrentAssembly())
				{
					lock (this)
					{
						pl.Init(serviceProvider);
					}
					return pl.GetPlugin<T>();
				}
			}
			throw new Exception("There is no Plugin in current assembly");
		}

		public T GetPlugin<T>(String name) where T : class, IPlugin
		{
			foreach (var pl in _plugs.Values)
			{
				if (!pl.IsValid) continue;
				if (pl.Name == name)
				{
					lock (this)
					{
						pl.Init(serviceProvider);
					}
					return pl.GetPlugin<T>();
				}
			}
			throw new Exception($"There is no Plugin {name}");
		}
	}
}
