using A2v10.ProcS.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A2v10.ProcS.Tests
{
	public static class ProcessEngine
	{
		public static (WorkflowEngine, IWorkflowStorage, IServiceBus) CreateEngine()
		{
			var storage = new FakeStorage();
			var mgr = new SagaManager();
			mgr.RegisterSagaFactory<ResumeProcess>(new ConstructSagaFactory<ProcessSaga>(nameof(ProcessSaga)));
			mgr.RegisterSagaFactory<CallApiRequestMessage, CallApiResponse>(new ConstructSagaFactory<CallHttpApiSaga>(nameof(CallHttpApiSaga)));
			mgr.RegisterSagaFactory<WaitCallbackMessage, CallbackMessage>(new ConstructSagaFactory<WaitApiCallbackSaga>(nameof(WaitApiCallbackSaga)));
			mgr.RegisterSagaFactory<WaitCallbackMessageProcess, CallbackMessageResume>(new ConstructSagaFactory<WaitApiCallbackProcessSaga>(nameof(WaitApiCallbackProcessSaga)));

			var keeper = new InMemorySagaKeeper(mgr);
			var scriptEngine = new ScriptEngine();
			var repository = new Repository(storage, storage);
			var bus = new ServiceBus(keeper, repository, scriptEngine);
			var engine = new WorkflowEngine(repository, bus, scriptEngine);
			return (engine, storage, bus);
		}

	}
}
