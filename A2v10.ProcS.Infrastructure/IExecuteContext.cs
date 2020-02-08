// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;
using System.Threading.Tasks;

namespace A2v10.ProcS.Infrastructure
{
	public interface IHandleContext 
	{
		Task<IInstance> LoadInstance(Guid id);
		void SendMessage(IMessage message);
		IScriptContext ScriptContext { get; }

		IResumeContext CreateResumeContext(IInstance instance);
		Task<IInstance> StartProcess(String processId, Guid parentId, IDynamicObject data = null);

		Task ResumeProcess(Guid id, IDynamicObject result);
		Task ResumeProcess(Guid id, String json);
	}

	public interface IExecuteContext : IHandleContext
	{
		IInstance Instance { get; }
		Task SaveInstance();

		String Resolve(String source);
		T EvaluateScript<T>(String expression);
		void ExecuteScript(String code);

		void ProcessComplete();
	}

	public interface IResumeContext : IExecuteContext
	{
		String Bookmark { get; }
		IDynamicObject Result { get; set; }
	}
}
