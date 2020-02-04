using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using A2v10.ProcS;

namespace A2v10.ProcS.Run
{
	class Program
	{
		static void Main()
		{
			var scriptEngine = new ScriptEngine();
			var bus = new ServiceBus(new InMemorySagaKeeper(), null, scriptEngine);
			var engine = new WorkflowEngine(null, bus, scriptEngine);

			while (true)
			{
				switch (Console.ReadKey().KeyChar)
				{
					case 'c':
						var instance = engine.CreateInstance(new Identity("program"));
						Console.WriteLine($"Instance created. Id={instance.Id}");
						break;
					case 'l':
						PrintList();
						break;
					case 't':
						break;
					case 'q':
						return;
				}
			}
		}

		static void PrintList()
		{

		}
	}
}
