using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace A2v10.ProcS.Interfaces
{
	public interface IServiceMessage
	{
		Task<Boolean> ExecuteAsync();
	}

	public interface IWorkflowServiceBus
	{
		void Send(IServiceMessage message);
		Task Run();
	}
}
