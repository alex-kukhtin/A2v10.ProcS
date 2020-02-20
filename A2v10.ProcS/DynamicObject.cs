// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using A2v10.ProcS.Infrastructure;
using Newtonsoft.Json.Serialization;
using System.Reflection;

namespace A2v10.ProcS
{
	internal class InterfaceContractResolver<T> : DefaultContractResolver where T : class
	{
		private readonly List<Type> types;

		public InterfaceContractResolver()
		{
			types = new List<Type>(new Type[] { typeof(T) });
		}

		protected override List<MemberInfo> GetSerializableMembers(Type objectType)
		{
			List<MemberInfo> list = null;
			var found = false;
			foreach (var t in types)
			{
				if (t.IsAssignableFrom(objectType))
				{
					list = base.GetSerializableMembers(t);
					found = true;
					break;
				}
			}
			if (!found) list = base.GetSerializableMembers(objectType);
			foreach (var itm in list)
			{
				if (itm.MemberType == MemberTypes.Property)
				{
					var type = (itm as PropertyInfo).PropertyType;
					if (type.IsInterface) types.Add(type);
				}
			}
			return list;
		}
	}

	public class DynamicObject : IDynamicObject
	{
		private readonly ExpandoObject _object;

		public Object Root => _object;

		public DynamicObject()
		{
			_object = new ExpandoObject();
		}

		public DynamicObject(ExpandoObject expando)
		{
			_object = expando ?? new ExpandoObject();
		}

		public static IDynamicObject From<T>(T data) where T : class
		{
			var settings = new JsonSerializerSettings();
			settings.ContractResolver = new InterfaceContractResolver<T>();
			settings.Converters.Add(new StringEnumConverter());
			var json = JsonConvert.SerializeObject(data, settings);
			return FromJson(json);
		}
		
		public static IDynamicObject From(ExpandoObject data)
		{
			return new DynamicObject(data);
		}

		public static IDynamicObject FromJson(String json)
		{
			if (String.IsNullOrEmpty(json))
				return new DynamicObject();
			return new DynamicObject(JsonConvert.DeserializeObject<ExpandoObject>(json, new ExpandoObjectConverter()));
		}

		public String ToJson()
		{
			return JsonConvert.SerializeObject(_object);
		}

		public void Set(String name, Object val)
		{
			Object valueToSet = val;
			switch (val)
			{
				case IDynamicObject doVal:
					valueToSet = doVal.Root;
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
				if (val is T tval) return tval;
				throw new Exception($"Field \"{name}\" is not \"{typeof(T)}\"");
			}
			else {
				throw new Exception($"There is no field \"{name}\" is DynamicObject");
			}
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

		public Boolean IsEmpty => (_object == null) || (_object as IDictionary<String, Object>).Count == 0;
	}
}
