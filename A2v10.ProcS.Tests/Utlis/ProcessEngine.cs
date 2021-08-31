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

			var configuration = new ConfigurationBuilder().Build();

			var scriptEngine = new ScriptEngine();
			var repository = new Repository(storage, storage, configuration);

			ProcS.RegisterActivities(rm);

			String pluginPath = GetPluginPath();
			pmr.LoadPlugins(pluginPath, configuration);
			
			var mgr = new SagaManager(null);
			ProcS.RegisterSagas(rm, mgr, scriptEngine, repository);
			pmr.RegisterResources(rm, mgr);

			var taskManager = new SyncTaskManager();
			var keeper = new InMemorySagaKeeper(mgr.Resolver);
			
			var notifyManager = new NotifyManager();

			var bus = new ServiceBus(taskManager, keeper, logger, notifyManager);

			var engine = new WorkflowEngine(repository, bus, scriptEngine, logger, notifyManager);
			return (engine, repository, bus);
		}

		static String GetPluginPath()
		{
			var path = Assembly.GetExecutingAssembly().Location;
			var pathes = path.Split(Path.DirectorySeparatorChar);
			var debugRelease = pathes[^3];
			var newPathes = pathes.Take(pathes.Length - 5).ToList();
			newPathes.Add($"A2v10.ProcS.Plugin");
			newPathes.Add($"bin");
			newPathes.Add(debugRelease);
			newPathes.Add("net5.0");
			return (!String.IsNullOrEmpty(newPathes[0]) ? String.Empty : new String(Path.DirectorySeparatorChar, 1)) + Path.Combine(newPathes.ToArray());
		}
	}
}
