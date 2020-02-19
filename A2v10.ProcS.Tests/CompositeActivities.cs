// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Threading.Tasks;
using A2v10.ProcS.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace A2v10.ProcS.Tests
{
	[TestClass]
	public class CompositeActivities
	{
		[TestMethod]
		public async Task SimpleSequence()
		{
			(IWorkflowEngine engine, _, ServiceBus bus) = ProcessEngine.CreateEngine();

			var prms = new DynamicObject();
			prms.Set("value", 1);

			var instance = await engine.StartWorkflow(new Identity("composite/sequence.json"), prms);

			await bus.Run(bus.CancelWhenEmpty.Token);

			Assert.AreEqual(null, instance.CurrentState);
			var r = instance.GetResult();
			Assert.AreEqual(37, r.Eval<Int32>("counter"));
		}
	}
}
