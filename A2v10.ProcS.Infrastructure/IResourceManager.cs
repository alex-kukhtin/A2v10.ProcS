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
		void RegisterResource<T>() where T : IStorable, new();
		void RegisterResources(IEnumerable<Type> types);
		void RegisterResources<T1,T2>()
			where T1 : IStorable, new()
			where T2 : IStorable, new();
		void RegisterResources<T1, T2, T3>()
			where T1 : IStorable, new()
			where T2 : IStorable, new()
			where T3 : IStorable, new();
		void RegisterResources<T1, T2, T3, T4>()
			where T1 : IStorable, new()
			where T2 : IStorable, new()
			where T3 : IStorable, new()
			where T4 : IStorable, new();
		void RegisterResourceFactory(String key, IResourceFactory factory);
	}

	public interface IResourceWrapper
	{
		IStorable Wrap(IStorable obj);
		IStorable Unwrap(IStorable res);
	}

	public interface IResourceFactory
	{
		IStorable Create();
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
}
