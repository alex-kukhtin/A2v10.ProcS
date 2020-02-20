// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;

namespace A2v10.ProcS.Infrastructure
{
	public interface IInstance: IStorable
	{
		Guid Id { get; }
		Guid ParentInstanceId { get; }
		Boolean IsComplete { get; set; }

		IWorkflowDefinition Workflow { get; set; }

		String CurrentState { get; set; }
		void SetState(String state);

		void SetParameters(IDynamicObject data);
		IDynamicObject GetParameters();

		void SetEnvironment(IDynamicObject data);

		IDynamicObject GetData();
		IDynamicObject GetResult();
	}
}
