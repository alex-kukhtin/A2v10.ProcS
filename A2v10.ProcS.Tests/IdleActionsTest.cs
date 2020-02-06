using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using A2v10.ProcS.Interfaces;

namespace A2v10.ProcS.Tests
{
	[TestClass]
	public class IdleActionTest
	{
		[TestMethod]
		public async Task SimpleCallApi()
		{
			(WorkflowEngine engine, IWorkflowStorage storage, IServiceBus bus) = ProcessEngine.CreateEngine();

			var wf = await storage.WorkflowFromStorage(new Identity("callapi.json")) as StateMachine;
			var stm = wf as StateMachine;
			Assert.IsInstanceOfType(stm.States["S1"].OnEntry, typeof(CallHttpApi));

			IInstance instance = await engine.Run(wf);

			await bus.Run();

			Assert.AreEqual("End", instance.CurrentState);

		}
	}
}
