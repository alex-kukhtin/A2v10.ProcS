﻿// Copyright © 2020 Alex Kukhtin. All rights reserved.

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
		Task ResumeProcess(Guid id, String result);
	}

	public interface IExecuteContext : IHandleContext
	{
		IInstance Instance { get; }
		Task SaveInstance();

		String Resolve(String source);
		T EvaluateScript<T>(String expression);
		void ExecuteScript(String code);
	}

	public interface IResumeContext : IExecuteContext
	{
		String Bookmark { get; }
		String Result { get; set; }
	}
}