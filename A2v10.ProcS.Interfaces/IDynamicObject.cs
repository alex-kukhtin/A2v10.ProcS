// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;
using System.Text;

namespace A2v10.ProcS.Interfaces
{
	public interface IDynamicObject
	{
		T Eval<T>(String expression, T fallback = default(T), Boolean throwIfError = false);
		Object RawValue { get; }
	}
}
