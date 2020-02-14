// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using Microsoft.Extensions.Configuration;

namespace A2v10.ProcS.Infrastructure
{
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]

	public sealed class ProcSPluginAttribute : Attribute
	{
		private Type pluginType;

		public IPlugin CreatePlugin()
		{
			if (pluginType == null) return null;
			return Activator.CreateInstance(pluginType) as IPlugin;
		}

		public ProcSPluginAttribute()
		{
			pluginType = null;
		}

		public ProcSPluginAttribute(Type plugin)
		{
			if (!typeof(IPlugin).IsAssignableFrom(plugin)) throw new Exception("Plugin class must implement IPlugin");
			pluginType = plugin;
		}
	}

	public interface IPlugin
	{
		void Init(IServiceProvider serviceProvider, IConfiguration configuration);
	}
}
