// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using A2v10.ProcS.Infrastructure;

namespace A2v10.ProcS
{
	public class Instance : IInstance
	{
		#region IInstance
		public Guid Id { get; set; }
		public Guid ParentInstanceId { get; set; }

		public Boolean IsComplete { get; set; }

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

		public IDynamicObject GetResult()
		{
			return Result;
		}
		#endregion

		public Instance()
		{
			Data = new DynamicObject();
			Parameters = new DynamicObject();
			Environment = new DynamicObject();
			Result = new DynamicObject();
		}

		public IDynamicObject Data { get; set; }
		public IDynamicObject Parameters { get; set; }
		public IDynamicObject Environment { get; set; }
		public IDynamicObject Result { get; }
	}
}
