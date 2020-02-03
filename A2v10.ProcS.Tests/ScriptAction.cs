using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using A2v10.ProcS.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace A2v10.ProcS.Tests
{
	[TestClass]
	public class ScriptAction
	{
		[TestMethod]
		public async Task SimpleCounter()
		{
			var storage = new FakeStorage();
			var bus = new ServiceBus(storage);
			var engine = new WorkflowEngine(storage, storage, bus);
			var data = new DynamicObject();
			data.Set("counter", 10);
			var instance = await engine.StartWorkflow(new Identity("counter.json"), data);
			var prms = instance.GetParameters().RawValue;

			Assert.AreEqual("End", instance.CurrentState);
			var d = prms as IDictionary<String, Object>;
			Assert.AreEqual((Double)(-1), d["counter"]);
		}
	}
}
