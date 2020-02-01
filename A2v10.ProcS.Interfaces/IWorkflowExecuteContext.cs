// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace A2v10.ProcS.Interfaces
{
	public interface IWorkflowExecuteContext
	{
		IWorkflowInstance Instance { get; }
		Task SaveInstance();
	}
}
