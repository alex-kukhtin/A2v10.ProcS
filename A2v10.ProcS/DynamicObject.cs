﻿// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using A2v10.ProcS.Infrastructure;

namespace A2v10.ProcS
{
	public class DynamicObject : IDynamicObject
	{
		private readonly ExpandoObject _object;

		public Object RawValue => _object;

		public DynamicObject()
		{
			_object = new ExpandoObject();
		}

		public DynamicObject(ExpandoObject expando)
		{
			_object = expando;
		}

		public static IDynamicObject From<T>(T data)
		{
			switch (data)
			{
				case ExpandoObject expando:
					return new DynamicObject(expando);
				case IDynamicObject dyna:
					return dyna;
				case String json:
					{
						if (String.IsNullOrEmpty(json))
							return new DynamicObject();
						return new DynamicObject(JsonConvert.DeserializeObject<ExpandoObject>(json, new ExpandoObjectConverter()));
					}
			}
			throw new NotImplementedException();
		}

		public void Set<T>(String name, T val)
		{
			Object valueToSet = val;
			switch (val)
			{
				case IDynamicObject doVal:
					valueToSet = doVal.RawValue;
					break;
			}
			var d = _object as IDictionary<String, Object>;
			if (d.ContainsKey(name))
				d[name] = valueToSet;
			else
				d.Add(name, valueToSet);
		}

		public T Get<T>(String name)
		{
			var d = _object as IDictionary<String, Object>;
			if (d.TryGetValue(name, out Object val))
			{
				if (val is T)
					return (T)val;
			}
			return default;
		}

		public T Eval<T>(String expression, T fallback = default, Boolean throwIfError = false)
		{
			if (expression == null)
				return fallback;
			var result = EvalExpression(expression, throwIfError);
			if (result == null)
				return fallback;
			return (T) Convert.ChangeType(result, typeof(T));
		}

		static readonly Regex _arrFind = new Regex(@"(\w+)\[(\d+)\]{1}", RegexOptions.Compiled);

		Object EvalExpression(String expression, Boolean throwIfError = false)
		{
			Object currentContext = _object;
			foreach (var exp in expression.Split('.'))
			{
				if (currentContext == null)
					return null;
				String prop = exp.Trim();
				var d = currentContext as IDictionary<String, Object>;
				if (prop.Contains("["))
				{
					var match = _arrFind.Match(prop);
					prop = match.Groups[1].Value;
					if ((d != null) && d.ContainsKey(prop))
					{
						if (d[prop] is IList<ExpandoObject> listExp)
							currentContext = listExp[Int32.Parse(match.Groups[2].Value)];
						else if (d[prop] is Object[] arrObj)
							currentContext = arrObj[Int32.Parse(match.Groups[2].Value)];
					}
					else
					{
						if (throwIfError)
							throw new ArgumentException($"Error in expression '{expression}'. Property '{prop}' not found");
						return null;
					}
				}
				else
				{
					if ((d != null) && d.ContainsKey(prop))
						currentContext = d[prop];
					else
					{
						if (throwIfError)
							throw new ArgumentException($"Error in expression '{expression}'. Property '{prop}' not found");
						return null;
					}
				}
			}
			return currentContext;
		}
	}
}
