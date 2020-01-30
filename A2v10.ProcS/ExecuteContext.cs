using A2v10.ProcS.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace A2v10.ProcS
{
	public class ExecuteContext
	{
		public IWorkflowInstance Instance { get; }
	}

	public class ContinueContext
	{
		public IWorkflowInstance Instance { get; }
		public String Bookmark { get; }
	}
}
