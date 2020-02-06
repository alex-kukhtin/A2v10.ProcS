using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using A2v10.ProcS.Infrastructure;

namespace A2v10.ProcS
{
	public class StateMachine : IWorkflowDefinition
	{
		public String Description { get; set; }
		public String InitialState { get; set; }

		public Dictionary<String, State> States { get; set; }

		private IIdentity _identity;

		public IIdentity GetIdentity() { return _identity; }
		public void SetIdentity(IIdentity identity) { _identity = identity; }

		public async Task Run(IExecuteContext context)
		{
			if (States == null || States.Count == 0)
				return;
			var instance = context.Instance;
			if (String.IsNullOrEmpty(instance.CurrentState))
			{
				if (!String.IsNullOrEmpty(InitialState))
					instance.CurrentState = InitialState;
				else
					instance.CurrentState = States.First(x => true).Key;
			}
			while (true)
			{
				if (States.TryGetValue(instance.CurrentState, out State state))
				{
					var result = await state.ExecuteStep(context);
					if (result != ExecuteResult.Continue)
						return;
				}
			}
		}

		public async Task Resume(IResumeContext context)
		{
			var instance = context.Instance;
			if (States.TryGetValue(instance.CurrentState, out State state))
			{
				await state.ContinueStep(context);
			}
			await Run(context);
		}
	}
}
