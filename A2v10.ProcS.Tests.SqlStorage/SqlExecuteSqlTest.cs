
using System;
using System.Threading.Tasks;
using A2v10.ProcS.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace A2v10.ProcS.Tests.SqlStorage
{
	[TestClass]
	[TestCategory("SqlServer storage")]
	public class SqlExecuteSqlTest
	{
		[TestMethod]
		public async Task ExecuteSql()
		{
			var (engine, repository, bus) = ProcessEngine.CreateSqlEngine();

			var prms = new DynamicObject();
			prms.Set("value", 5);
			var instance = await engine.StartWorkflow(new Identity("executesql/simple.json"), prms);
			var id = instance.Id;

			await bus.Process();

			instance = await repository.Get(id);

			Assert.AreEqual(null, instance.CurrentState);
		}
	}
}
