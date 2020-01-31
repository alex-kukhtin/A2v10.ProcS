using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using A2v10.ProcS.Interfaces;

namespace A2v10.ProcS
{
	public class StopMessage : IServiceMessage
	{
		public Task<Boolean> ExecuteAsync()
		{
			return Task.FromResult(false);
		}
	}

	public class WorkflowServiceBus : IWorkflowServiceBus
	{
		ConcurrentQueue<IServiceMessage> _messages = new ConcurrentQueue<IServiceMessage>();

		public void Send(IServiceMessage message) 
		{
			_messages.Enqueue(message);
		}

		// void -> Fire and forget
		public async Task Run()
		{
			while (_messages.TryDequeue(out IServiceMessage message))
			{
				if (!await message.ExecuteAsync())
					return;
			}
		}
	}
}
