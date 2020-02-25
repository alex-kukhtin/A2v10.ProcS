// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using A2v10.ProcS.Infrastructure;
using Newtonsoft.Json.Serialization;
using System.Reflection;
using System.Dynamic;
using DynamicObject = A2v10.ProcS.Infrastructure.DynamicObject;

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

	public static class DynamicObjectConverters
	{
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

		public static String ToJson(this IDynamicObject obj)
		{
			if (obj == null || obj.IsEmpty)
				return null;
			return JsonConvert.SerializeObject(obj.Root);
		}

		public static T To<T>(this IDynamicObject obj) where T : class
		{
			if (obj == null || obj.IsEmpty)
				return null;
			return JsonConvert.DeserializeObject<T>(obj.ToJson());
		}
	}
}
