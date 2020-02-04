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
		public static IWorkflowEngine CreateEngine()
		{
			var storage = new FakeStorage();
			var keeper = new InMemorySagaKeeper();
			var scriptEngine = new ScriptEngine();
			var repository = new Repository(storage, storage);
			var bus = new ServiceBus(keeper, repository, scriptEngine);
			var engine = new WorkflowEngine(repository, bus, scriptEngine);
			return engine;
		}

	}
}
