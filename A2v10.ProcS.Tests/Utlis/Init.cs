using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A2v10.ProcS.Tests
{
	[TestClass]
	public class Init
	{
		[AssemblyInitialize]
		public static void AssemblyInitialize(TestContext context)
		{
			WorkflowEngine.RegisterSagas();
		}

		[AssemblyCleanup]
		public static void AssemblyCleanup()
		{
		}
	}
}
