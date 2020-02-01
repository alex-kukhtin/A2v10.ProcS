using System;
using System.Collections.Generic;
using System.Text;

namespace A2v10.ProcS.Interfaces
{
	public interface IWorkflowInstance
	{
		Guid Id { get; }
		String CurrentState { get; set; }

		void SetState(String state);
	}
}
