using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using A2v10.ProcS.Infrastructure;

namespace A2v10.ProcS.Tests
{
	[TestClass]
	public class IdleActionTest
	{
		[TestMethod]
		public async Task SimpleCallApi()
		{
			(WorkflowEngine engine, IWorkflowStorage storage, IServiceBus bus) = ProcessEngine.CreateEngine();

			var instance = await engine.StartWorkflow("callapi.json");

			var stm = instance.Workflow as StateMachine;
			Assert.IsInstanceOfType(stm.States["S1"].OnEntry, typeof(CallHttpApi));


			await bus.Run();

			Assert.AreEqual("End", instance.CurrentState);

		}
	}
}
