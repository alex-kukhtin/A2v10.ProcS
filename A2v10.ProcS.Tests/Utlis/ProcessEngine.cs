// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using A2v10.ProcS.Infrastructure;
using Microsoft.Extensions.Configuration;

namespace A2v10.ProcS.Tests
{
	public static class ProcessEngine
	{
		public static (WorkflowEngine engine, IRepository repository, InMemoryServiceBus bus) CreateEngine()
		{
			var storage = new FakeStorage();
			var pmr = new PluginManager(null);

			String pluginPath = GetPluginPath();
			var configuration = new ConfigurationBuilder().Build();
			pmr.LoadPlugins(pluginPath, configuration);

			var rm = new ResourceManager(null);
			
			var mgr = new SagaManager(null);
			ProcS.RegisterSagas(rm, mgr);
			pmr.RegisterResources(rm, mgr);

			

			var taskManager = new SyncTaskManager();
			var keeper = new InMemorySagaKeeper(mgr.Resolver);
			var scriptEngine = new ScriptEngine();
			var repository = new Repository(storage, storage);
			var bus = new InMemoryServiceBus(taskManager, keeper, repository, scriptEngine);
			var engine = new WorkflowEngine(repository, bus, scriptEngine);
			return (engine, repository, bus);
		}

		static String GetPluginPath()
		{
			var path = new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath;
			var pathes = path.Split(Path.DirectorySeparatorChar);
			var debugRelease = pathes[^3];
			var newPathes = pathes.Take(pathes.Length - 5).ToList();
			newPathes.Add($"A2v10.ProcS.Plugin");
			newPathes.Add($"bin");
			newPathes.Add(debugRelease);
			newPathes.Add("netstandard2.0");
			return (newPathes[0] == "" ? new String(Path.DirectorySeparatorChar, 1) : "") + Path.Combine(newPathes.ToArray());
		}
	}
}
