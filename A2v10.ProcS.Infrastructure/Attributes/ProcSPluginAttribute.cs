// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using Microsoft.Extensions.Configuration;

namespace A2v10.ProcS.Infrastructure
{
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]

	public sealed class ProcSPluginAttribute : Attribute
	{
		public IPlugin CreatePlugin()
		{
			if (PluginType == null) return null;
			return Activator.CreateInstance(PluginType) as IPlugin;
		}

		public Type PluginType { get; }

		public String Name { get; }

		public ProcSPluginAttribute(String name)
		{
			Name = name;
			PluginType = null;
		}

		public ProcSPluginAttribute(String name, Type plugin) : this(name)
		{
			if (!typeof(IPlugin).IsAssignableFrom(plugin)) throw new Exception("Plugin class must implement IPlugin");
			PluginType = plugin;
		}
	}

	public interface IPlugin
	{
		void Init(IServiceProvider serviceProvider, IConfiguration configuration);
	}
}
