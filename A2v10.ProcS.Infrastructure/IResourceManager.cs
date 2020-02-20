// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace A2v10.ProcS.Infrastructure
{
	public interface IResourceManager
	{
		void RegisterResource<T>() where T : IStorable, new();
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
