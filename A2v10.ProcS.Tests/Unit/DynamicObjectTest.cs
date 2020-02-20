using System;
using System.Collections.Generic;
using System.Text;
using A2v10.ProcS.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace A2v10.ProcS.Tests.Unit
{
	[TestClass]
	public class DynamicObjectTest
	{
		[TestMethod]
		public void GetSetSimple()
		{
			IDynamicObject dyna = new DynamicObject();
			dyna.Set("String", "String");
			dyna.Set("Int32", 33);
			dyna.Set("Boolean", true);

			Assert.AreEqual("String", dyna.Get<String>("String"));
			Assert.AreEqual(33, dyna.Get<Int32>("Int32"));
			Assert.AreEqual(true, dyna.Get<Boolean>("Boolean"));
		}

		[TestMethod]
		public void GetTypeConversion()
		{
			IDynamicObject dyna = new DynamicObject();
			dyna.Set("Int32", 33);

			Assert.AreEqual("33", dyna.Get<String>("Int32"));
			Assert.AreEqual(33, dyna.Get<Int32>("Int32"));
			Assert.AreEqual(33M, dyna.Get<Decimal>("Int32"));
		}

		[TestMethod]
		public void SimpleJson()
		{
			IDynamicObject dyna = new DynamicObject();
			dyna.Set("String", "String");
			dyna.Set("Int32", 33);
			dyna.Set("Boolean", true);
			String json = dyna.ToJson();

			IDynamicObject dyna2 = DynamicObject.FromJson(json);
			Assert.AreEqual(dyna2.Get<String>("String"), dyna.Get<String>("String"));
			Assert.AreEqual(dyna2.Get<Int32>("Int32"), dyna.Get<Int32>("Int32"));
			Assert.AreEqual(dyna2.Get<Boolean>("Boolean"), dyna.Get<Boolean>("Boolean"));
		}
	}
}
