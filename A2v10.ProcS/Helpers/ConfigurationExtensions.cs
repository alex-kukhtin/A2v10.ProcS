// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Extensions.Configuration;
using A2v10.ProcS.Infrastructure;

namespace A2v10.ProcS
{
	public static class ConfigurationExtensions
	{
		public static IDynamicObject AsDynamic(this IConfiguration config)
		{
			if (config == null)
				return null;
			var d = new DynamicObject();
			foreach (var ch in config.GetChildren())
			{
				Object val = ch.Value;
				if (val == null)
					val = ch.AsDynamic();
				d.Add(ch.Key, val);
			}
			return d;
		}
	}
}
