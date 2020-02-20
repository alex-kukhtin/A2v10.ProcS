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
using System.Collections;
using System.Linq.Expressions;

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
		private ExpandoObject _object;

#pragma warning disable IDE1006 // Naming Styles
		private IDictionary<String, Object> _dictionary => _object;
#pragma warning restore IDE1006 // Naming Styles

		public static implicit operator DynamicObject(ExpandoObject dobj)
		{
			return new DynamicObject(dobj);
		}

		public static implicit operator ExpandoObject(DynamicObject dobj)
		{
			return dobj.Root;
		}

		public ExpandoObject Root => _object;

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
			if (data is ExpandoObject eo)
				return new DynamicObject(eo);
			var settings = new JsonSerializerSettings()
			{
				ContractResolver = new InterfaceContractResolver<T>()
			};
			settings.Converters.Add(new StringEnumConverter());
			var json = JsonConvert.SerializeObject(data, settings);
			return FromJson(json);
		}
		
		public static IDynamicObject From(ExpandoObject data)
		{
			return new DynamicObject(data);
		}

		public static IDynamicObject From(IDynamicObject data)
		{
			return FromJson(data.ToJson());
		}

		public static IDynamicObject FromJson(String json)
		{
			if (String.IsNullOrEmpty(json) || json == "{}")
				return new DynamicObject();
			return new DynamicObject(JsonConvert.DeserializeObject<ExpandoObject>(json, new ExpandoObjectConverter()));
		}

		public void AssignFrom(String name, IDynamicObject source)
		{
			var dobj = source.GetDynamicObject(name);
			if (dobj != null)
				_object = dobj.Root;
			else
				Clear();
		}

		public String ToJson()
		{
			if (IsEmpty)
				return null;
			return JsonConvert.SerializeObject(_object);
		}

		public void Set(String name, Object val)
		{
			Object valueToSet = val;
			switch (val)
			{
				case IDynamicObject doVal:
					if (doVal.IsEmpty)
						return;
					valueToSet = doVal.Root;
					break;
			}
			if (this.ContainsKey(name))
				this[name] = valueToSet;
			else
				this.Add(name, valueToSet);
		}

		public T Get<T>(String name)
		{
			if (this.TryGetValue(name, out Object val))
			{
				if (val is T tval) 
					return tval;
				return (T)Convert.ChangeType(val, typeof(T));
			}
			else {
				throw new Exception($"There is no field \"{name}\" is DynamicObject");
			}
		}

		public List<T> GetListOrNull<T>(String name)
		{
			if (this.TryGetValue(name, out Object val))
			{
				if (val is IList<Object> listObj)
				{
					var list = new List<T>();
					foreach (var el in listObj)
					{
						var ne = (T)Convert.ChangeType(el, typeof(T));
						list.Add(ne);
					}
					return list;
				}
			}
			return null;
		}

		public IDynamicObject GetDynamicObject(String name)
		{
			if (this.TryGetValue(name, out Object val))
			{
				if (val is ExpandoObject eo)
					return new DynamicObject(eo);
				else if (val is DynamicObject dobj)
					return dobj;
				else
					throw new Exception($"The field \"{name}\" is not a DynamicObject");
			}
			return null;
		}

		public T GetOrDefault<T>(String name)
		{
			if (this.TryGetValue(name, out Object val))
			{
				if (val is T tval) 
					return tval;
				return default;
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

		public void Add(String key, Object value)
		{
			if (value is IDynamicObject dobj)
				value = dobj.Root;
			_dictionary.Add(key, value);
		}

		public Boolean ContainsKey(String key)
		{
			return _dictionary.ContainsKey(key);
		}

		public Boolean Remove(String key)
		{
			return _dictionary.Remove(key);
		}

		public Boolean TryGetValue(String key, out Object value)
		{
			return _dictionary.TryGetValue(key, out value);
		}

		public void Add(KeyValuePair<String, Object> item)
		{
			_dictionary.Add(item);
		}

		public void Clear()
		{
			_dictionary.Clear();
		}

		public Boolean Contains(KeyValuePair<String, Object> item)
		{
			return _dictionary.Contains(item);
		}

		public void CopyTo(KeyValuePair<String, Object>[] array, Int32 arrayIndex)
		{
			_dictionary.CopyTo(array, arrayIndex);
		}

		public Boolean Remove(KeyValuePair<String, Object> item)
		{
			return _dictionary.Remove(item);
		}

		public IEnumerator<KeyValuePair<String, Object>> GetEnumerator()
		{
			return _dictionary.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _dictionary.GetEnumerator();
		}

		public DynamicMetaObject GetMetaObject(Expression parameter)
		{
			return (_object as IDynamicMetaObjectProvider).GetMetaObject(parameter);
		}

		public Boolean IsEmpty => (_object == null) || (_object as IDictionary<String, Object>).Count == 0;

		public ICollection<String> Keys => _dictionary.Keys;

		public ICollection<Object> Values => _dictionary.Values;

		public Int32 Count => _dictionary.Count;

		public Boolean IsReadOnly => _dictionary.IsReadOnly;

		public Object this[String key] {
			get => _dictionary[key];
			set { _dictionary[key] = value; }
		}
	}
}
