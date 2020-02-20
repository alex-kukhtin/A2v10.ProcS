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
			(IWorkflowEngine engine, _, InMemoryServiceBus bus) = ProcessEngine.CreateEngine();

			var prms = new DynamicObject();
			prms.Set("value", 1);

			var instance = await engine.StartWorkflow(new Identity("composite/sequence.json"), prms);

			bus.Process();

			Assert.AreEqual(null, instance.CurrentState);
			var r = instance.GetResult();
			Assert.AreEqual(37, r.Eval<Int32>("counter"));
		}

		[TestMethod]
		public async Task SimpleParallel()
		{
			(IWorkflowEngine engine, _, InMemoryServiceBus bus) = ProcessEngine.CreateEngine();

			var prms = new DynamicObject();
			prms.Set("value", 1);

			var instance = await engine.StartWorkflow(new Identity("composite/parallel.json"), prms);

			bus.Process();

			Assert.AreEqual(null, instance.CurrentState);
			var r = instance.GetResult();
			Assert.AreEqual(4, r.Eval<Int32>("counter"));
		}

		[TestMethod]
		public async Task ParallelApi()
		{
			(IWorkflowEngine engine, IRepository repository, InMemoryServiceBus bus) = ProcessEngine.CreateEngine();

			var prms = new DynamicObject();
			prms.Set("value", 1);

			var instance = await engine.StartWorkflow(new Identity("composite/parallelApi.json"), prms);
			var id = instance.Id;

			bus.Process();

			instance = await repository.Get(id);
			Assert.AreEqual(null, instance.CurrentState);
			var r = instance.GetResult();
			Assert.AreEqual(15, r.Eval<Int32>("counter"));
		}
	}
}
