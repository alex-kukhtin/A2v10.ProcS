// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A2v10.ProcS.Tests
{
	[TestClass]
	public class CallApiTest
	{
		[TestMethod]
		public async Task OpenWeatherApi()
		{
			var (engine, repository, bus) = ProcessEngine.CreateEngine();

			var prms = new DynamicObject();
			prms.Set("city", "London");
			var instance = await engine.StartWorkflow(new Identity("callapi/openweather.json"), prms);
			var id = instance.Id;

			bus.Process();

			instance = await repository.Get(id);
			var result = instance.GetResult();
			Assert.AreEqual(7.0, result.Eval<Double>("temp"));
			Assert.AreEqual("London", instance.GetResult().Eval<String>("city"));

			Assert.AreEqual(null, instance.CurrentState);
		}
	}
}
