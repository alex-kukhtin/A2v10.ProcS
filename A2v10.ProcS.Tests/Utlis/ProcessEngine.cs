using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using A2v10.ProcS.Infrastructure;

namespace A2v10.ProcS.Tests
{
	public static class ProcessEngine
	{
		public static (WorkflowEngine engine, IWorkflowStorage storage, IServiceBus bus) CreateEngine()
		{
			var storage = new FakeStorage();
			var mgr = new SagaManager(null);

			mgr.RegisterSagaFactory<ResumeProcessMessage>(new ConstructSagaFactory<ProcessSaga>(nameof(ProcessSaga)));
			mgr.RegisterSagaFactory<StartProcessMessage>(new ConstructSagaFactory<ProcessSaga>(nameof(ProcessSaga)));

			mgr.RegisterSagaFactory<CallApiRequestMessage, CallApiResponse>(new ConstructSagaFactory<CallHttpApiSaga>(nameof(CallHttpApiSaga)));
			mgr.RegisterSagaFactory<WaitCallbackMessage, CallbackMessage>(new ConstructSagaFactory<WaitApiCallbackSaga>(nameof(WaitApiCallbackSaga)));
			mgr.RegisterSagaFactory<WaitCallbackMessageProcess, CallbackMessageResume>(new ConstructSagaFactory<WaitApiCallbackProcessSaga>(nameof(WaitApiCallbackProcessSaga)));

			String pluginPath = GetPluginPath();

			mgr.LoadPlugins(pluginPath);

			var keeper = new InMemorySagaKeeper(mgr);
			var scriptEngine = new ScriptEngine();
			var repository = new Repository(storage, storage);
			var bus = new ServiceBus(keeper, repository, scriptEngine);
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
