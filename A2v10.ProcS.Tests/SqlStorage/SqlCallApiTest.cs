
using System;
using System.Threading.Tasks;
using A2v10.ProcS.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace A2v10.ProcS.Tests
{
	[TestClass]
	[TestCategory("SqlServer storage")]
	public class SqlCallApiTest
	{
		[TestMethod]
		public async Task OpenWeatherApi()
		{
			var (engine, repository, bus) = ProcessEngine.CreateSqlEngine();

			var prms = new DynamicObject();
			prms.Set("city", "London");
			var instance = await engine.StartWorkflow(new Identity("callapi/openweather.json"), prms);
			var id = instance.Id;

			await bus.Process();

			instance = await repository.Get(id);
			var result = instance.GetResult();
			Assert.AreEqual(7.0, result.Eval<Double>("temp"));
			Assert.AreEqual("London", instance.GetResult().Eval<String>("city"));

			Assert.AreEqual(null, instance.CurrentState);
		}
	}
}
