// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;

namespace A2v10.ProcS.Interfaces
{
	public interface IScriptEngine
	{
		IScriptContext CreateContext();
	}

	public interface IScriptContext : IDisposable
	{
		T Eval<T>(String expression);
		void Execute(String code);
		void SetValue(String name, IDynamicObject value);
		T GetValueFromJson<T>(String json, String expression);
	}
}
