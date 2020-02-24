// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using A2v10.ProcS.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace A2v10.ProcS.Tests
{
	[TestClass]
	public class PluginLoadTest
	{
		[TestMethod]
		public async Task LoadPlugin()
		{
			(IWorkflowEngine engine, IRepository repository, ServiceBus bus) = ProcessEngine.CreateEngine();

			IInstance inst = await engine.StartWorkflow(new Identity("plugins/loadplugin.json"));
			var id = inst.Id;

			await bus.Process();

			inst = await repository.Get(id);
			Assert.AreEqual(42, inst.GetResult().Eval<Int32>("value"));
			Assert.AreEqual(null, inst.CurrentState);
		}
	}
}
