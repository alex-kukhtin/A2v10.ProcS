using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json;

namespace A2v10.ProcS.Tests
{
	[TestClass]
	public class SimpleStateMachine
	{
		[TestMethod]
		public void LoadDefinition()
		{

			var storage = new FakeStorage();
			var stm = storage.WorkflowFromStorage("simple.json") as StateMachine;

			Assert.AreEqual("S1", stm.InitialState);
			Assert.AreEqual("First state machine", stm.Description);

			Assert.AreEqual(3, stm.States.Count);

			var s1 = stm.States["S1"];

			Assert.AreEqual("State 1", s1.Description);
			Assert.AreEqual(1, s1.Transitions.Count);

			var t1 = s1.Transitions["S1->S2"];
			Assert.AreEqual("S2", t1.To);
			Assert.AreEqual(true, t1.Default);
			Assert.AreEqual("From S1 to S2", t1.Description);
		}

		[TestMethod]
		public async Task SimpleRun()
		{
			var storage = new FakeStorage();

			var stm = storage.WorkflowFromStorage("simple.json");

			var bus = new WorkflowServiceBus();
			var engine = new WorkflowEngine(storage, storage, bus);

			await engine.Run(stm);
		}
	}
}
