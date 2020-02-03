// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;
using A2v10.ProcS.Interfaces;
using Jint;

namespace A2v10.ProcS
{
	public class ScriptEngine : IScriptEngine
	{
		private readonly Engine _engine = new Engine((opts)=>
		{
			opts.Strict(true);
		});

		public T Eval<T>(String expression)
		{

			var val = _engine.Execute(expression).GetCompletionValue();
			var vo = val.ToObject();
			return (T) Convert.ChangeType(vo, typeof(T));
		}
	}
}
