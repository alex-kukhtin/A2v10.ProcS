using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using A2v10.ProcS.Infrastructure;


namespace A2v10.ProcS.Tests
{
	[TestClass]
	public class WaitCallback
	{
		
		[TestMethod]
		public async Task SimpleWait()
		{
			(WorkflowEngine engine, _, IServiceBus bus) = ProcessEngine.CreateEngine();

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
