using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;

namespace A2v10.ProcS
{
	public class DynamicObject
	{
		private readonly ExpandoObject _object;

		public DynamicObject()
		{
			_object = new ExpandoObject();
		}

		public void Set(String name, Object val)
		{
			var d = _object as IDictionary<String, Object>;
			if (d.ContainsKey(name))
				d[name] = val;
			else
				d.Add(name, val);
		}

		public T Get<T>(String name)
		{
			var d = _object as IDictionary<String, Object>;
			if (d.TryGetValue(name, out Object val))
			{
				if (val is T)
					return (T)val;
			}
			return default(T);
		}
	}
}
