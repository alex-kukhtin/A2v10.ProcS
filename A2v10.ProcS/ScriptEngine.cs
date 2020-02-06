// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;
using A2v10.ProcS.Infrastructure;
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
			if (expression == null)
				return default;
			var val = _engine.Execute(expression).GetCompletionValue();
			var vo = val.ToObject();
			return (T) Convert.ChangeType(vo, typeof(T));
		}

		public void Execute(String code)
		{
			if (code == null)
				return;
			_engine.Execute(code);
		}

		public void SetValue(String name, IDynamicObject val)
		{
			_engine.SetValue(name, val.RawValue);
		}

		public T GetValueFromObject<T>(IDynamicObject obj, String expression)
		{
			if (obj == null || expression == null)
				return default;
			var val = Jint.Native.JsValue.FromObject(_engine, obj.RawValue);
			var func = _engine.Execute($"(reply) => ({expression})").GetCompletionValue();
			var result = func.Invoke(val).ToObject();
			if (result is T)
				return (T)result;
			return (T) Convert.ChangeType(result, typeof(T));
		}
	}
}
