using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Threading.Tasks;
using A2v10.ProcS.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace A2v10.ProcS.Tests
{
	[TestClass]
	public class WaitCallback
	{
		
		[TestMethod]
		public async Task SimpleWait()
		{
			var storage = new FakeStorage();
			var keeper = new InMemorySagaKeeper();
			var bus = new ServiceBus(keeper, storage);
			var engine = new WorkflowEngine(storage, storage, bus);

			var data = new DynamicObject();
			var instance = await engine.StartWorkflow(new Identity("callback.json"), data);

			var resp = new CallbackMessage("pseudopay");
			resp.Result = "{ \"paymentId\": 123 }";
			bus.Send(resp);
			await bus.Run();

			Assert.AreEqual("End", instance.CurrentState);
		}
	}
}
