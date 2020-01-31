using System;
using System.Collections.Generic;
using System.Text;
using A2v10.ProcS.Interfaces;

namespace A2v10.ProcS
{
	public class WorkflowInstance
	{
		public Guid Id { get; set; }
		public String CurrentState { get; set; }

		public Boolean IsComplete { get; set; }

		public DynamicObject Data { get; set; }
		public DynamicObject Parameters { get; set; }
		public DynamicObject Environment { get; set; }
	}
}
