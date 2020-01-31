using System;
using System.IO;
using System.Threading.Tasks;
using A2v10.ProcS.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json;

namespace A2v10.ProcS.Tests
{
	[TestClass]
	public class DelayAction
	{
		[TestMethod]
		public async Task SimpleRun()
		{
			var stm = Startup.Load("delay.json");
			Assert.AreEqual("S1", stm.InitialState);
			Assert.AreEqual("Delay Test", stm.Description);
			var s1 = stm.States["S1"];
			Assert.IsInstanceOfType(s1.OnEntry, typeof(A2v10.ProcS.Delay));

			var bus = new WorkflowServiceBus();
			var engine = new WorkflowEngine(bus);

			await engine.Run(stm);
		}
	}
}
