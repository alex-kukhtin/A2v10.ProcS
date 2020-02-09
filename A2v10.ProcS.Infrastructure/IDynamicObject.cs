// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;

namespace A2v10.ProcS.Infrastructure
{
	public interface IDynamicObject
	{
		T Eval<T>(String expression, T fallback = default, Boolean throwIfError = false);
		void Set<T>(String name, T val);
		Object RawValue { get; }
	}
}
