// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace A2v10.ProcS.Tests.SqlStorage
{
	[TestClass]
	public class WaitCallbackTest
	{

		[TestMethod]
		public async Task SimpleWait()
		{
			var (engine, repository, bus) = ProcessEngine.CreateSqlEngine();

			var data = engine.CreateDynamicObject();
			var instance = await engine.StartWorkflow(new Identity("callback.json"), data);
			var id = instance.Id;

			var resp = new CallbackMessage("pseudopay") {
				Result = DynamicObjectConverters.FromJson("{ \"paymentId\": 123 }")
			};

			bus.Send(resp);
			await bus.Process();

			instance = await repository.Get(id);
			Assert.AreEqual(null, instance.CurrentState);
		}
	}
}
