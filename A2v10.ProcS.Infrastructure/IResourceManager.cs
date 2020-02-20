// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace A2v10.ProcS.Infrastructure
{
	public interface IResourceManager
	{
		void RegisterResource(Type type);
		void RegisterResource<T>() where T : new();
		void RegisterResources(IEnumerable<Type> types);
		void RegisterResources(params Type[] types);
		void RegisterResources<T1,T2>()
			where T1 : new()
			where T2 : new();
		void RegisterResources<T1, T2, T3>()
			where T1 : new()
			where T2 : new()
			where T3 : new();
		void RegisterResources<T1, T2, T3, T4>()
			where T1 : new()
			where T2 : new()
			where T3 : new()
			where T4 : new();
		void RegisterResourceFactory(String key, IResourceFactory factory);
	}

	public interface IResourceWrapper
	{
		IStorable Wrap(Object obj);
		Object Unwrap(IStorable res);
		T Unwrap<T>(IStorable res) where T : class;
	}

	public interface IResourceFactory
	{
		Object Create(IDynamicObject data);
	}

	[AttributeUsage(AttributeTargets.Class)]
	public class ResourceKeyAttribute : Attribute
	{
		public ResourceKeyAttribute(String key)
		{
			Key = key;
		}

		public String Key { get; }
	}

	[AttributeUsage(AttributeTargets.Constructor)]
	public class RestoreWithAttribute : Attribute
	{
		public RestoreWithAttribute()
		{
			
		}
	}
}
