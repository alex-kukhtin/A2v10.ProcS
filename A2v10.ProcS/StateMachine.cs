using System;
using System.Collections.Generic;
using System.Text;

namespace A2v10.ProcS
{
	public class StateMachine
	{
		public Dictionary<String, State> States { get; set; }

		public void Run(ActivityContext context)
		{
			String currState = "";
			while (true)
			{
				if (States.TryGetValue(currState, out State state))
				{
					state.ExecuteStep(context);
				}
			}
		}
	}
}
