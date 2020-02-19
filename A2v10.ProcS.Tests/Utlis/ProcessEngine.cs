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
		public static (WorkflowEngine engine, IWorkflowStorage storage, ServiceBus bus) CreateEngine()
		{
			var storage = new FakeStorage();
			var mgr = new SagaManager(null);

			mgr.RegisterSagaFactory<SetBookmarkMessage, ResumeBookmarkMessage>(new ConstructSagaFactory<BookmarkSaga>(nameof(BookmarkSaga)));
			mgr.RegisterSagaFactory<StartProcessMessage, ContinueActivityMessage>(new ConstructSagaFactory<ProcessSaga>(nameof(ProcessSaga)));

			mgr.RegisterSagaFactory<CallApiRequestMessage, CallApiResponseMessage>(new ConstructSagaFactory<CallHttpApiSaga>(nameof(CallHttpApiSaga)));
			mgr.RegisterSagaFactory<RegisterCallbackMessage, CallbackMessage>(new ConstructSagaFactory<RegisterCallbackSaga>(nameof(RegisterCallbackSaga)));
			mgr.RegisterSagaFactory<WaitCallbackMessage, CorrelatedCallbackMessage>(new ConstructSagaFactory<CallbackCorrelationSaga>(nameof(CallbackCorrelationSaga)));

			String pluginPath = GetPluginPath();

			var configuration = new ConfigurationBuilder().Build();

			mgr.LoadPlugins(pluginPath, configuration);

			var taskManager = new SyncTaskManager();
            var keeper = new InMemorySagaKeeper(mgr.Resolver);
			var scriptEngine = new ScriptEngine();
			var repository = new Repository(storage, storage);
			var bus = new ServiceBus(taskManager, keeper, repository, scriptEngine);
			var engine = new WorkflowEngine(repository, bus, scriptEngine);
			return (engine, storage, bus);
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
