// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;
using A2v10.ProcS.Interfaces;
using Jint;

namespace A2v10.ProcS
{
	public class ScriptEngine : IScriptEngine
	{
		public IScriptContext CreateContext()
		{
			return new ScriptContext();
		}
	}

	public class ScriptContext : IScriptContext
	{
		private readonly Engine _engine = new Engine((opts)=>
		{
			opts.Strict(true);
		});

		public void Dispose()
		{
			Dispose(true);
		}

		protected virtual void Dispose(Boolean disposing)
		{
		}

		public T Eval<T>(String expression)
		{

			var val = _engine.Execute(expression).GetCompletionValue();
			var vo = val.ToObject();
			return (T) Convert.ChangeType(vo, typeof(T));
		}

		public void Execute(String code)
		{
			_engine.Execute(code);
		}

		public void SetValue(String name, IDynamicObject val)
		{
			_engine.SetValue(name, val.RawValue);
		}
	}
}
