﻿// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using A2v10.ProcS.Infrastructure;
using Microsoft.Extensions.Configuration;

namespace A2v10.ProcS
{
	public class TypeResourceFactory : IResourceFactory
	{
		private readonly Type type;

		public TypeResourceFactory(Type type)
		{
			this.type = type;
		}

		public Object Create()
		{
			return Activator.CreateInstance(type);
		}
	}

	public class GenericResourceFactory<T> : IResourceFactory where T : new()
	{
		public Object Create()
		{
			return new T();
		}
	}

	public class SagaResourceFactory : IResourceFactory
	{
		private readonly ISagaFactory fact;

		public SagaResourceFactory(ISagaFactory fact)
		{
			this.fact = fact;
		}

		public Object Create()
		{
			return fact.CreateSaga();
		}
	}

	public class ResourceManager : IResourceManager, IResourceWrapper
	{
		protected readonly Dictionary<String, IResourceFactory> resources = new Dictionary<String, IResourceFactory>();
		protected readonly IServiceProvider serviceProvider;

		public ResourceManager(IServiceProvider serviceProvider)
		{
			this.serviceProvider = serviceProvider;
		}

		public void RegisterResourceFactory(String key, IResourceFactory factory)
		{
			resources.Add(key, factory);
		}

		private String GetKey(Type type)
		{
			var att = type.GetCustomAttribute<ResourceKeyAttribute>();
			if (att == null) throw new Exception("Resource must have ResourceKeyAttribute");
			return att.Key;
		}

		public void RegisterResource(Type type)
		{
			RegisterResourceFactory(GetKey(type), new TypeResourceFactory(type));
		}

		public void RegisterResource<T>(String key) where T : new()
		{
			RegisterResourceFactory(key, new GenericResourceFactory<T>());
		}

		public void RegisterResource<T>() where T : new()
		{
			RegisterResource<T>(GetKey(typeof(T)));
		}

		public void RegisterResources(IEnumerable<Type> types)
		{
			foreach (var t in types)
			{
				RegisterResource(t);
			}
		}

		public void RegisterResources<T1, T2>()
			where T1 : new()
			where T2 : new()
		{
			RegisterResource<T1>();
			RegisterResource<T2>();
		}

		public void RegisterResources<T1, T2, T3>()
			where T1 : new()
			where T2 : new()
			where T3 : new()
		{
			RegisterResource<T1>();
			RegisterResource<T2>();
			RegisterResource<T3>();
		}

		public void RegisterResources<T1, T2, T3, T4>()
			where T1 : new()
			where T2 : new()
			where T3 : new()
			where T4 : new()
		{
			RegisterResource<T1>();
			RegisterResource<T2>();
			RegisterResource<T3>();
			RegisterResource<T4>();
		}

		public IStorable Wrap(Object obj)
		{
			var type = obj.GetType();
			var att = type.GetCustomAttribute<ResourceKeyAttribute>();
			if (att == null) throw new Exception("Resource must have ResourceKeyAttribute");
			IDynamicObject data;
			if (obj is IStorable sto) data = sto.Store();
			else data = new DynamicObject();
			return new Resource(att.Key, data);
		}

		public Object Unwrap(IStorable src)
		{
			var res = RestoreResource(src);
			var obj = Unwrap(res);
			return obj;
		}

		private Resource RestoreResource(IStorable src)
		{
			var d = src.Store();
			var res = new Resource();
			res.Restore(d);
			return res;
		}

		private Object Unwrap(Resource res)
		{
			var key = res.Key;
			if (!resources.ContainsKey(key))
				throw new Exception($"Resource {key} is not registred");
			var fact = resources[key];
			var obj = fact.Create();
			if (obj is IStorable sto)
				sto.Restore(res.Object);
			return obj;
		}

		public T Unwrap<T>(IStorable src) where T : class
		{
			var res = RestoreResource(src);
			var obj = Unwrap(res);
			if (obj is T t) return t;
			throw new Exception($"Resource {res.Key} is not {typeof(T)}");
		}
	}

	public class Resource : IStorable
	{
		public Resource()
		{

		}

		public Resource(String key, IDynamicObject obj)
		{
			Key = key;
			Object = obj;
		}

		public String Key { get; private set; }
		public IDynamicObject Object { get; private set; }

		private const string key = "$res";

		public IDynamicObject Store()
		{
			var d = DynamicObject.From(Object);
			d.Set(key, Key);
			return d;
		}

		public void Restore(IDynamicObject store)
		{
			var data = DynamicObject.From(store);
			Key = data.Get<String>(key);
			data.Remove(key);
			Object = data;
		}
	}
}
