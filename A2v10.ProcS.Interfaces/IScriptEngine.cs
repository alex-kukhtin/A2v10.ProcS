using System;
using System.Collections.Generic;
using System.Text;

namespace A2v10.ProcS.Interfaces
{
	public interface IScriptEngine
	{
		T Eval<T>(String expression);
	}
}
