// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using A2v10.ProcS.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace A2v10.ProcS.Tests.SqlStorage
{
	[TestClass]
	public class DelayAction
	{
		[TestMethod]
		public async Task SimpleRun()
		{
			var (engine, repository, bus) = ProcessEngine.CreateSqlEngine();

			var instance = await engine.StartWorkflow("delay.json");
			var stm = instance.Workflow as StateMachine;

			await bus.Process();

			await Task.Delay(1500);
			await bus.Process();

			var ni = await repository.Get(instance.Id);


			Assert.AreEqual("S1", stm.InitialState);
			Assert.AreEqual("Delay Test", stm.Description);
			var s1 = stm.States["S1"];
			Assert.IsInstanceOfType(s1.OnEntry, typeof(A2v10.ProcS.Delay));

			Assert.IsNull(ni.CurrentState);
		}
	}
}
