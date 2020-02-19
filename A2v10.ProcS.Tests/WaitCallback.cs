// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

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
			(WorkflowEngine engine, _, InMemoryServiceBus bus) = ProcessEngine.CreateEngine();

			var data = engine.CreateDynamicObject();
			var instance = await engine.StartWorkflow(new Identity("callback.json"), data);

			var resp = new CallbackMessage("pseudopay") {
				Result = DynamicObject.From("{ \"paymentId\": 123 }")
			};

			bus.Send(resp);
			bus.Process();

			Assert.AreEqual(null, instance.CurrentState);
		}
	}
}
