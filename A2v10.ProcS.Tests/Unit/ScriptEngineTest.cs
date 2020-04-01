using A2v10.ProcS.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text;

namespace A2v10.ProcS.Tests.Unit
{
	[TestClass]
	public class ScriptEngineTest
	{
		[TestMethod]
		public void DoubleJsonConverter()
		{
			var engine = new ScriptEngine();
			using (var context = engine.CreateContext())
			{
				context.SetValue("v", new DynamicObject()
				{
					{"x", 5},
					{"y", 7}
				});

				var r = context.EvalObject("({x: v.x + 1, y: v.y + 1})");

				var x = r.Eval<Object>("x");
				var y = r.Eval<Object>("y");

				var rjson = JsonConvert.SerializeObject(r, new DoubleConverter());

				Assert.AreEqual(6.0, x);
				Assert.AreEqual(8.0, y);

				// without .0!
				Assert.AreEqual("{\"x\":6,\"y\":8}", rjson);
			}
		}
	}
}
