// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Collections.Generic;
using System.Reflection;

using A2v10.ProcS.Infrastructure;

namespace A2v10.ProcS
{
	public class TypeResourceFactory : IResourceFactory
	{
		private readonly Type type;
		private readonly (ConstructorInfo c, ParameterInfo[] prms) ct;

		public TypeResourceFactory(Type type)
		{
			this.type = type;
			var cts = type.GetConstructors();
			var found = false;
			foreach (var ct in cts)
			{
				var att = ct.GetCustomAttribute<RestoreWithAttribute>();
				if (att != null)
				{
					found = true;
					this.ct = (ct, ct.GetParameters());
				}
			}
			if (!found)
			{
				foreach (var ct in cts)
				{
					var p = ct.GetParameters();
					if (p.Length == 0)
					{
						found = true;
						this.ct = (ct, ct.GetParameters());
					}
				}
			}
			if (!found) 
				throw new Exception("Resource must have Constructor without parameters or Constructor marked with RestoreWithAttribute");
		}

		public Object Create(IDynamicObject data)
		{
			var prms = new Object[ct.prms.Length];
			var i = 0;
			foreach (var p in ct.prms)
			{
				if (!data.ContainsKey(p.Name))
					throw new Exception($"There is no value for constructor parameter '{p.Name}'");
				var dt = data[p.Name];
				if (dt != null)
				{
					if (!p.ParameterType.IsAssignableFrom(dt.GetType()))
						dt = DynamicObject.ConvertTo(dt, p.ParameterType);
					prms[i] = dt;
				}
				i++;
			}
			return Activator.CreateInstance(type, prms);
		}
	}

	public class GenericResourceFactory<T> : IResourceFactory where T : new()
	{
		public Object Create(IDynamicObject data)
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

		public Object Create(IDynamicObject data)
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

		public void RegisterResources(params Type[] types)
		{
			RegisterResources(types as IEnumerable<Type>);
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
			if (obj == null) return new Empty();
			var type = obj.GetType();
			var att = type.GetCustomAttribute<ResourceKeyAttribute>();
			if (att == null) 
				throw new InvalidProgramException("Resource must have ResourceKeyAttribute");
			IDynamicObject data;
			if (obj is IStorable sto) 
				data = sto.Store(this);
			else 
				data = new DynamicObject();
			return new Resource(att.Key, data);
		}

		public Object Unwrap(IStorable src)
		{
			var res = RestoreResource(src);
			var obj = Unwrap(res);
			return obj;
		}

		public Object Unwrap(IDynamicObject obj)
		{
			var res = RestoreResource(obj);
			var robj = Unwrap(res);
			return robj;
		}

		private Resource RestoreResource(IDynamicObject obj)
		{
			if (obj == null || obj.IsEmpty) return null;
			var res = new Resource();
			res.Restore(obj, this);
			return res;
		}

		private Resource RestoreResource(IStorable src)
		{
			var d = src?.Store(this);
			return RestoreResource(d);
		}

		public Object Create(String key, IDynamicObject data)
		{
			if (!resources.ContainsKey(key))
				throw new Exception($"Resource {key} is not registred");
			var fact = resources[key];
			return fact.Create(data);
		}

		public T Create<T>(String key, IDynamicObject data) where T : class
		{
			var obj = Create(key, data);
			return Generalyze<T>(obj, key);
		}

		public Object Unwrap(Resource res)
		{
			if (res == null) return null;
			var key = res.Key;
			if (!resources.ContainsKey(key))
				throw new Exception($"Resource {key} is not registred");
			var fact = resources[key];
			var obj = fact.Create(res.Object);
			if (obj is IStorable sto)
				sto.Restore(res.Object, this);
			return obj;
		}

		private T Generalyze<T>(Object obj, String key) where T : class
		{
			if (obj == null) return null;
			if (obj is T t) return t;
			throw new Exception($"Resource {key} is not {typeof(T)}");
		}

		public T Unwrap<T>(IStorable src) where T : class
		{
			var res = RestoreResource(src);
			var obj = Unwrap(res);
			return Generalyze<T>(obj, res?.Key);
		}

		public T Unwrap<T>(Resource res) where T : class
		{
			var obj = Unwrap(res);
			return Generalyze<T>(obj, res?.Key);
		}

		public T Unwrap<T>(IDynamicObject obj) where T : class
		{
			var res = RestoreResource(obj);
			var robj = Unwrap(res);
			return Generalyze<T>(robj, res?.Key);
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

		private const String key = "$res";

		public IDynamicObject Store(IResourceWrapper _)
		{
			var d = DynamicObjectConverters.From(Object);
			d.Set(key, Key);
			return d;
		}

		public void Restore(IDynamicObject store, IResourceWrapper _)
		{
			var data = DynamicObjectConverters.From(store);
			Key = data.Get<String>(key);
			data.Remove(key);
			Object = data;
		}
	}

	public class Empty : IStorable
	{
		public Empty()
		{

		}

		public IDynamicObject Store(IResourceWrapper _)
		{
			return new DynamicObject();
		}

		public void Restore(IDynamicObject store, IResourceWrapper _)
		{
			throw new InvalidOperationException("Can't restore emptiness...");
		}
	}
}
