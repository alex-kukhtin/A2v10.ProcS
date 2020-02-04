using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using A2v10.ProcS.Interfaces;

namespace A2v10.ProcS.Tests
{
	[TestClass]
	public class IdleActionTest
	{
		[TestMethod]
		public async Task SimpleCallApi()
		{
			var storage = new FakeStorage();
			var keeper = new InMemorySagaKeeper();
			var scriptEngine = new ScriptEngine();
			var repository = new Repository(storage, storage);

			var wf = await storage.WorkflowFromStorage(new Identity("callapi.json")) as StateMachine;
			var stm = wf as StateMachine;
			Assert.IsInstanceOfType(stm.States["S1"].OnEntry, typeof(CallHttpApi));

			var bus = new ServiceBus(keeper, repository, scriptEngine);

			var engine = new WorkflowEngine(repository, bus, scriptEngine);
			IInstance instance = await engine.Run(wf);

			await bus.Run();

			Assert.AreEqual("End", instance.CurrentState);

		}
	}
}
