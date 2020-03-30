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
			var (engine, repository, bus) = ProcessEngine.CreateEngine();

			var data = engine.CreateDynamicObject();
			var instance = await engine.StartWorkflow(new Identity("callback.json"), data);
			var id = instance.Id;

			var resp = new CallbackMessage("pseudopay") {
				Result = DynamicObjectConverters.FromJson("{ \"paymentId\": 123 }")
			};

			bus.Send(resp);
			await bus.Process();

			instance = await repository.Get(id);
			Assert.AreEqual(null, instance.CurrentState);
		}

		[TestMethod]
		public async Task RespWait()
		{
			var (engine, repository, bus) = ProcessEngine.CreateEngine();

			var data = engine.CreateDynamicObject();
			var instance = await engine.StartWorkflow(new Identity("callback_wait.json"), data);
			var id = instance.Id;

			var prms = DynamicObjectConverters.FromJson("{ \"cb\": { \"id\": 555 } }");
			var instance2 = await engine.StartWorkflow(new Identity("callback_send.json"), prms);

			await bus.Process();

			instance = await repository.Get(id);
			Assert.AreEqual(null, instance.CurrentState);
		}
	}
}
