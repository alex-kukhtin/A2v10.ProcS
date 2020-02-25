// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using Microsoft.Extensions.Configuration;
using A2v10.Data.Interfaces;

namespace A2v10.ProcS.Tests
{
	public class DatabaseConfig : IDataConfiguration
	{
		private readonly IConfiguration _config;
		public DatabaseConfig(IConfiguration config)
		{
			_config = config;
		}

		public String ConnectionString(String source)
		{
			if (String.IsNullOrEmpty(source))
				source = "Default";
			return _config.GetConnectionString(source);
		}
	}
}
