// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;

namespace A2v10.ProcS.Infrastructure
{
	public interface IInstance
	{
		Guid Id { get; }
		IWorkflowDefinition Workflow { get; set; }

		String CurrentState { get; set; }
		void SetState(String state);

		void SetParameters(IDynamicObject data);
		IDynamicObject GetParameters();

		void SetEnvironment(IDynamicObject data);

		IDynamicObject GetData();
		IDynamicObject GetResult();

		//IDynamicObject GetData();
		//void SetData(IDynamicObject data);
	}
}
