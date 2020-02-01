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
	public class IdleAction
	{
		[TestMethod]
		public async Task SimpleCallApi()
		{
			var storage = new FakeStorage();

			var wf = await storage.WorkflowFromStorage(new Identity("idle.json"));

			var stm = wf as StateMachine;
			Assert.IsInstanceOfType(stm.States["S1"].OnEntry, typeof(CallHttpApi));

			var bus = new WorkflowServiceBus(storage);

			var engine = new WorkflowEngine(storage, storage, bus);
			IInstance instance = await engine.Run(wf);

			await bus.Run();

			Assert.AreEqual("End", instance.CurrentState);

		}
	}
}
