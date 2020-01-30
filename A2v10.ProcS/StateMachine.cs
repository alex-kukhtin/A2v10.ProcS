using System;
using System.Collections.Generic;
using System.Text;

namespace A2v10.ProcS
{
	public class StateMachine
	{
		public Dictionary<String, State> States { get; set; }

		public void Run(ExecuteContext context)
		{
			while (true)
			{
				if (States.TryGetValue(context.Instance.CurrentState, out State state))
				{
					state.ExecuteStep(context);
				}
			}
		}
	}
}
