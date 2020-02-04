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
			var bus = new ServiceBus(new InMemorySagaKeeper(), null);
			var engine = new WorkflowEngine(null, null, bus);

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
