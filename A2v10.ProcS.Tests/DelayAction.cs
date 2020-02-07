using System;
using System.IO;
using System.Threading.Tasks;
using A2v10.ProcS.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace A2v10.ProcS.Tests
{
	[TestClass]
	public class DelayAction
	{
		[TestMethod]
		public async Task SimpleRun()
		{
			var (engine, storage, _) = ProcessEngine.CreateEngine();

			var stm = (await storage.WorkflowFromStorage(new Identity("delay.json"))) as StateMachine;

			Assert.AreEqual("S1", stm.InitialState);
			Assert.AreEqual("Delay Test", stm.Description);
			var s1 = stm.States["S1"];
			Assert.IsInstanceOfType(s1.OnEntry, typeof(A2v10.ProcS.Delay));

			await engine.Run(stm);
		}
	}
}
