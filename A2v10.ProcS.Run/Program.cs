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
		static void Main(String[] args)
		{
			var bus = new WorkflowServiceBus();
			var engine = new WorkflowEngine(bus);

			while (true)
			{
				switch (Console.ReadKey().KeyChar)
				{
					case 'c':
						var instance = new WorkflowInstance();
						Console.WriteLine($"Instance created. Id={instance.Id}");
						break;
					case 'l':
						PrintList(engine);
						break;
					case 't':
						break;
					case 'q':
						return;
				}
			}
		}

		static void PrintList(WorkflowEngine engine)
		{

		}
	}
}
