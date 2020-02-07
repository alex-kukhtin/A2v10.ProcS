using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using A2v10.ProcS.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace A2v10.ProcS.Tests
{
	[TestClass]
	public class PluginLoadTest
	{
		[TestMethod]
		public async Task LoadPlugin()
		{
			/*
var path = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
path = Path.Combine(path, "_plugins/A2v10.ProcS.Plugin.dll");
String path2 = @"D:\Git\A2v10.ProcS\A2v10.ProcS.Plugin\bin\Debug\netstandard2.0\A2v10.ProcS.Plugin.dll";
*/
			// var ass =  Assembly.LoadFile(path2);

			(IWorkflowEngine engine, _, IServiceBus bus) = ProcessEngine.CreateEngine();

			IInstance inst = await engine.StartWorkflow(new Identity("plugins/loadplugin.json"));

			await bus.Run();

			Assert.AreEqual("End", inst.CurrentState);
			//Assert.AreEqual(42, inst.GetData().Eval<Int32>("result"));

		}
	}
}
