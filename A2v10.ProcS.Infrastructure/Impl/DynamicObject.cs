using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text.RegularExpressions;
using System.Collections;
using System.Linq.Expressions;

namespace A2v10.ProcS.Infrastructure
{
	public class DynamicObject : IDynamicObject
	{
		private ExpandoObject _object;

		private IDictionary<String, Object> _dictionary => _object;

		public ExpandoObject Root => _object;

		public static implicit operator DynamicObject(ExpandoObject dobj)
		{
			return new DynamicObject(dobj);
		}

		public static implicit operator ExpandoObject(DynamicObject dobj)
		{
			return dobj.Root;
		}

		public DynamicObject()
		{
			_object = new ExpandoObject();
		}

		public DynamicObject(ExpandoObject expando)
		{
			_object = expando ?? new ExpandoObject();
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
				return ConvertTo<T>(val);
			}
			else
			{
				throw new Exception($"There is no field \"{name}\" is DynamicObject");
			}
		}

		public static T ConvertTo<T>(Object val)
		{
			return (T)ConvertTo(val, typeof(T));
		}

		public static Object ConvertTo(Object val, Type type)
		{
			switch (val)
			{
				case String strVal:
					{
						if (type.IsAssignableFrom(typeof(Guid)))
						{
							return Guid.Parse(strVal);
						}
						break;
					}
			}
			return Convert.ChangeType(val, type);
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

		public void AssignFrom(String name, IDynamicObject source)
		{
			var dobj = source.GetDynamicObject(name);
			if (dobj != null)
				_object = dobj.Root;
			else
				Clear();
		}

		private static IEnumerable<T> EnumerableConvert<T>(IEnumerable en)
		{
			foreach (var el in en)
			{
				var ne = ConvertTo<T>(el);
				yield return ne;
			}
		}

		public IEnumerable<T> GetEnumerableOrNull<T>(String name)
		{
			if (this.TryGetValue(name, out Object val))
			{
				if (val is IEnumerable en)
				{
					return EnumerableConvert<T>(en);
				}
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
			return (T)Convert.ChangeType(result, typeof(T));
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

		public void Add(string key, object value)
		{
			_dictionary.Add(key, value);
		}

		public bool ContainsKey(string key)
		{
			return _dictionary.ContainsKey(key);
		}

		public bool Remove(string key)
		{
			return _dictionary.Remove(key);
		}

		public bool TryGetValue(string key, out object value)
		{
			return _dictionary.TryGetValue(key, out value);
		}

		public void Add(KeyValuePair<string, object> item)
		{
			_dictionary.Add(item);
		}

		public void Clear()
		{
			_dictionary.Clear();
		}

		public bool Contains(KeyValuePair<string, object> item)
		{
			return _dictionary.Contains(item);
		}

		public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
		{
			_dictionary.CopyTo(array, arrayIndex);
		}

		public bool Remove(KeyValuePair<string, object> item)
		{
			return _dictionary.Remove(item);
		}

		public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
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

		public Boolean IsEmpty => (_object == null) || (_object as IDictionary<string, object>).Count == 0;

		public ICollection<string> Keys => _dictionary.Keys;

		public ICollection<object> Values => _dictionary.Values;

		public int Count => _dictionary.Count;

		public bool IsReadOnly => _dictionary.IsReadOnly;

		public object this[string key]
		{
			get => _dictionary[key];
			set { _dictionary[key] = value; }
		}
	}
}
