// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;
using A2v10.ProcS.Interfaces;

namespace A2v10.ProcS
{
	public class WorkflowInstance : IInstance
	{
		public Guid Id { get; set; }
		public String CurrentState { get; set; }

		public IWorkflowDefinition Workflow { get; set; }

		public Boolean IsComplete { get; set; }

		public DynamicObject Data { get; set; }
		public DynamicObject Parameters { get; set; }
		public DynamicObject Environment { get; set; }

		public void SetState(String state)
		{
			CurrentState = state;
		}
	}
}
