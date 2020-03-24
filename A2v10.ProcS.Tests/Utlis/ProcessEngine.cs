// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;

using A2v10.ProcS.Infrastructure;
using Microsoft.Extensions.Logging;

namespace A2v10.ProcS.Tests
{
	public static class ProcessEngine
	{
		static ILogger CreateLogger()
		{
			using var factory = LoggerFactory.Create(builder => builder.AddConsole());
			return factory.CreateLogger<IWorkflowEngine>();
		}

		public static (WorkflowEngine engine, IRepository repository, ServiceBus bus) CreateEngine()
		{
			var rm = new ResourceManager(null);

			var storage = new FakeStorage(rm);
			var pmr = new PluginManager(null);
			var logger = CreateLogger();

			String pluginPath = GetPluginPath();
			var configuration = new ConfigurationBuilder().Build();
			pmr.LoadPlugins(pluginPath, configuration);
			
			var mgr = new SagaManager(null);
			ProcS.RegisterSagas(rm, mgr);
			pmr.RegisterResources(rm, mgr);

			var taskManager = new SyncTaskManager();
			var keeper = new InMemorySagaKeeper(mgr.Resolver);
			var scriptEngine = new ScriptEngine();
			var repository = new Repository(storage, storage);
			var bus = new ServiceBus(taskManager, keeper, repository, scriptEngine, logger);

			var engine = new WorkflowEngine(repository, bus, scriptEngine, logger);
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
			return (!String.IsNullOrEmpty(newPathes[0]) ? String.Empty : new String(Path.DirectorySeparatorChar, 1)) + Path.Combine(newPathes.ToArray());
		}
	}
}
