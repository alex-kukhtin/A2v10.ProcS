// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using A2v10.ProcS.Infrastructure;
using System.Threading;

namespace A2v10.ProcS.Tests
{
	[TestClass]
	public class IdleActionTest
	{
		[TestMethod]
		public async Task SimpleCallApi()
		{
			var (engine, repository, bus) = ProcessEngine.CreateEngine();

			var instance = await engine.StartWorkflow("callapi.json");
			var id = instance.Id;

			var stm = instance.Workflow as StateMachine;
			Assert.IsInstanceOfType(stm.States["S1"].OnEntry, typeof(CallHttpApiActivity));

			await bus.Process(CancellationToken.None);

			instance = await repository.Get(id);
			Assert.AreEqual(null, instance.CurrentState);

		}
	}
}
