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
			(IWorkflowEngine engine, _, ServiceBus bus) = ProcessEngine.CreateEngine();

			IInstance inst = await engine.StartWorkflow(new Identity("plugins/loadplugin.json"));

			await bus.Run(bus.CancelWhenEmpty.Token);

			Assert.AreEqual("End", inst.CurrentState);
			Assert.AreEqual(42, inst.GetResult().Eval<Int32>("value"));
		}
	}
}
