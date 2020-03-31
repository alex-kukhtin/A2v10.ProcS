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
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging;

namespace A2v10.ProcS.Tests.SqlStorage
{
	public static class ProcessEngine
	{

		static ILogger<IWorkflowEngine> CreateLogger()
		{
			using var factory = LoggerFactory.Create(builder => builder.AddConsole());
			return factory.CreateLogger<IWorkflowEngine>();
		}

		public static (WorkflowEngine engine, IRepository repository, ServiceBus bus) CreateSqlEngine()
		{
			var fullPath = Path.GetFullPath("../../../tests.config.json");

			var configuration = new ConfigurationBuilder()
				.AddJsonFile(fullPath)
				.AddUserSecrets<DatabaseConfig>()
				.Build();

			var rm = new ResourceManager(null);
			var mgr = new SagaManager(null);


			var scriptEngine = new ScriptEngine();
			var profiler = new NullDataProfiler();
			var localizer = new NullDataLocalizer();
			var dbConfig = new DatabaseConfig(configuration);
			var dbContext = new SqlDbContext(profiler, dbConfig, localizer);
			var workflowStorage = new FileSystemWorkflowStorage(rm);
			var taskManager = new SyncTaskManager();
			var logger = CreateLogger();

			
			var instanceStorage = new SqlServerInstanceStorage(workflowStorage, dbContext, rm, logger);
			var repository = new Repository(workflowStorage, instanceStorage);

			ProcS.RegisterActivities(rm);
			ProcS.RegisterSagas(rm, mgr, scriptEngine, repository);
			SqlServerProcS.RegisterActivities(rm);
			SqlServerProcS.RegisterSagas(rm, mgr, dbContext);

			var keeper = new SqlServerSagaKeeper(mgr.Resolver, dbContext, rm);

			
			var notifyManager = new NotifyManager();

			var bus = new ServiceBus(taskManager, keeper, logger, notifyManager); 

			var engine = new WorkflowEngine(repository, bus, scriptEngine, new NullLogger<IWorkflowEngine>(), notifyManager);
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
