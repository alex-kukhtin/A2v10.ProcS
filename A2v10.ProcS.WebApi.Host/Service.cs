// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using A2v10.ProcS.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace A2v10.ProcS.WebApi.Host
{
	public class Service : BackgroundService
	{
		private ServiceBusAsync bus;

		public IResourceManager ResourceManager { get; }
		public ISagaManager SagaManager { get; }
		public IPluginManager PluginManager { get; }
		public IServiceBus ServiceBus => bus;

		public Service(IResourceManager rm, ISagaManager sm, PluginManager pm, ServiceBusAsync sb, IConfiguration conf)
		{
			ProcS.RegisterActivities(rm);
			ProcS.RegisterSagas(rm, sm);

			foreach (var path in GetPluginPathes(conf))
			{
				pm.LoadPlugins(path, conf.GetSection("ProcS:Plugins"));
			}

			pm.RegisterResources(rm, sm);

			ResourceManager = rm;
			SagaManager = sm;
			PluginManager = pm;
			bus = sb;
		}

		private static IEnumerable<String> GetPluginPathes(IConfiguration conf)
		{
			return conf.GetSection("ProcS:PluginsPath").GetChildren().Select(s => s.Value);
		}

		protected override Task ExecuteAsync(CancellationToken stoppingToken)
		{
			return bus.Run(stoppingToken);
		}
	}
}
