// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;

using A2v10.Data;
using A2v10.Data.Interfaces;
using A2v10.ProcS.Infrastructure;
using A2v10.ProcS.SqlServer;

namespace A2v10.ProcS.Tests.SqlStorage
{
	public static class ProcessEngine
	{
		public static (WorkflowEngine engine, IRepository repository, ServiceBus bus) CreateSqlEngine()
		{
			var fullPath = Path.GetFullPath("../../../tests.config.json");

			var configuration = new ConfigurationBuilder()
				.AddJsonFile(fullPath)
				.Build();

			var profiler = new NullDataProfiler();
			var localizer = new NullDataLocalizer();
			var dbConfig = new DatabaseConfig(configuration);
			var dbContext = new SqlDbContext(profiler, dbConfig, localizer);
			var workflowStorage = new FileSystemWorkflowStorage();
			var instanceStorage = new SqlServerInstanceStorage(workflowStorage, dbContext);
			var repository = new Repository(workflowStorage, instanceStorage);

			var taskManager = new SyncTaskManager();
			var rm = new ResourceManager(null);

			var mgr = new SagaManager(null);
			ProcS.RegisterSagas(rm, mgr);

			var keeper = new SqlServerSagaKeeper(mgr.Resolver, dbContext, rm);

			var scriptEngine = new ScriptEngine();
			var bus = new ServiceBus(taskManager, keeper, repository, scriptEngine);

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
			return (!String.IsNullOrEmpty(newPathes[0]) ? String.Empty : new String(Path.DirectorySeparatorChar, 1)) + Path.Combine(newPathes.ToArray());
		}
	}
}
