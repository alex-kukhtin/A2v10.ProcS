// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;

namespace A2v10.ProcS.Infrastructure
{
	public interface IDynamicObject
	{
		T Eval<T>(String expression, T fallback = default, Boolean throwIfError = false);
		void Set(String name, Object val);
		T Get<T>(String name);

		String ToJson();
		Boolean IsEmpty { get; }

		Object Root { get; }
	}
}
