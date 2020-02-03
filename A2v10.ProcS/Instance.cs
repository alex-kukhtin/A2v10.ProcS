// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;
using A2v10.ProcS.Interfaces;

namespace A2v10.ProcS
{
	public class Instance : IInstance
	{
		#region IInstance
		public Guid Id { get; set; }

		public IWorkflowDefinition Workflow { get; set; }

		public String CurrentState { get; set; }

		public void SetState(String state)
		{
			CurrentState = state;
		}
		public void SetParameters(IDynamicObject data)
		{
			if (data != null)
				Parameters = data;
		}

		public IDynamicObject GetParameters()
		{
			return Parameters;
		}

		public void SetEnvironment(IDynamicObject env)
		{
			if (env != null)
				Environment = env;
		}

		public IDynamicObject GetData()
		{
			return Data;
		}
		#endregion

		public Boolean IsComplete { get; set; }

		public Instance()
		{
			Data = new DynamicObject();
			Parameters = new DynamicObject();
			Environment = new DynamicObject();
		}

		public IDynamicObject Data { get; set; }
		public IDynamicObject Parameters { get; set; }
		public IDynamicObject Environment { get; set; }
	}
}
