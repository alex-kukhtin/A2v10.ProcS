using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace A2v10.ProcS.Tests
{
	[TestClass]
	public class IdleAction
	{
		[TestMethod]
		public void SimpleCallApi()
		{
			var storage = new FakeStorage();
			var wf = storage.WorkflowFromStorage("idle.json");
			var stm = wf as StateMachine;

			Assert.IsInstanceOfType(stm.States["S1"].OnEntry, typeof(CallHttpApi));

			var bus = new WorkflowServiceBus();
		}
	}
}
