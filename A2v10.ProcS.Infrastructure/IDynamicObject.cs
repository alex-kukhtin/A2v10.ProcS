// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Collections.Generic;
using System.Dynamic;

namespace A2v10.ProcS.Infrastructure
{
	public interface IDynamicObject : IDictionary<String, Object>, IDynamicMetaObjectProvider
	{
		T Eval<T>(String expression, T fallback = default, Boolean throwIfError = false);
		void Set(String name, Object val);
		T Get<T>(String name);
		T GetOrDefault<T>(String name);
		List<T> GetListOrNull<T>(String name);

		IDynamicObject GetDynamicObject(String name);

		void AssignFrom(String name, IDynamicObject from);

		Boolean IsEmpty { get; }

		ExpandoObject Root { get; }
	}
}
