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
			var engine = new WorkflowEngine(null);

			while (true)
			{
				switch (Console.ReadKey().KeyChar)
				{
					case 's':
						engine.Create("1");
						break;
					case 'l':
						break;
					case 't':
						break;
					case 'q':
						return;
				}
			}
		}
	}
}
