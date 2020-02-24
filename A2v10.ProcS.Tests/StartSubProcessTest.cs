// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using A2v10.ProcS.Infrastructure;

namespace A2v10.ProcS.Tests
{
	[TestClass]
	public class StartSubProcessTest
	{
		[TestMethod]
		public async Task ChildProcessSimple()
		{
			// master p1 = p1, p2 = p1 * 2
			// slave p1 = p1 + 5, p2 = p2 + 10;

			(WorkflowEngine engine, IRepository repository, ServiceBus bus) = ProcessEngine.CreateEngine();
			var prms = engine.CreateDynamicObject();
			prms.Set("value", 10);
			var instance = await engine.StartWorkflow("startprocess/master.json", prms);
			var id = instance.Id;

			await bus.Process();

			instance = await repository.Get(id);
			Assert.AreEqual(15, instance.GetResult().Eval<Int32>("value.p1"));
			Assert.AreEqual(30, instance.GetResult().Eval<Int32>("value.p2"));
		}

		[TestMethod]
		public async Task ChildProcessCorrelation()
		{
			// master p1 = p1, p2 = p1 * 2
			// slave p1 = p1 + 5, p2 = p2 + 10;

			(WorkflowEngine engine, IRepository repository, ServiceBus bus) = ProcessEngine.CreateEngine();
			
			var prms1 = engine.CreateDynamicObject();
			prms1.Set("value", 10);
			var instance1 = await engine.StartWorkflow("startprocess/master.json", prms1);
			var id1 = instance1.Id;

			var prms2 = engine.CreateDynamicObject();
			prms2.Set("value", 20);
			var instance2 = await engine.StartWorkflow("startprocess/master.json", prms2);
			var id2 = instance2.Id;

			await bus.Process();

			instance1 = await repository.Get(id1);
			instance2 = await repository.Get(id2);

			Assert.AreEqual(15, instance1.GetResult().Eval<Int32>("value.p1"));
			Assert.AreEqual(30, instance1.GetResult().Eval<Int32>("value.p2"));

			Assert.AreEqual(25, instance2.GetResult().Eval<Int32>("value.p1"));
			Assert.AreEqual(50, instance2.GetResult().Eval<Int32>("value.p2"));
		}
	}
}
