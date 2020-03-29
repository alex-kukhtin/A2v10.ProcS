// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using A2v10.ProcS.Infrastructure;
using A2v10.ProcS.WebApi.Host.Classes;
using A2v10.ProcS.SqlServer;
using A2v10.Data;
using A2v10.Data.Interfaces;

namespace A2v10.ProcS.WebApi.Host
{
	using Host = Microsoft.Extensions.Hosting.Host;

	public static class Program
	{

		public static void Main(String[] args)
		{
			CreateHostBuilder(args).Build().Run();
		}

		private static ILogger CreateLogger()
		{
			using var factory = LoggerFactory.Create(builder => builder.AddConsole());
			return factory.CreateLogger<IWorkflowEngine>();
		}

		private class DatabaseConfig : IDataConfiguration
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

		public static IHostBuilder CreateHostBuilder(String[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureLogging(logging => {
					logging.ClearProviders();
					logging.AddConsole();
				})
				.ConfigureServices((ctx, services) =>
				{
					var conf = ctx.Configuration;

					services.AddHostedService<Service>();

					var dbc = new DatabaseConfig(conf);

					services.AddSingleton<IDataProfiler, NullDataProfiler>();
					services.AddSingleton<IDataConfiguration>(dbc);
					services.AddSingleton<IDataLocalizer, NullDataLocalizer>();
					services.AddSingleton<IDbContext, SqlDbContext>();

					services.AddSingleton<ILogger, FakeLogger>();

					var tm = new TaskManager();
					services.AddSingleton<ITaskManager>(tm);

					var cat = new FilesystemWorkflowCatalogue(conf["ProcS:Workflows"]);
					services.AddSingleton<IWorkflowCatalogue>(cat);
					var epm = new EndpointManager();

					services.AddSingleton<IEndpointManager>(epm);
					services.AddSingleton<IEndpointResolver>(epm);

					services.AddSingleton<IWorkflowStorage, SqlServerWorkflowStorage>();
					services.AddSingleton<IInstanceStorage, SqlServerInstanceStorage>();

					services.AddSingleton<IScriptEngine, ScriptEngine>();
					services.AddSingleton<IRepository, Repository>();
					services.AddSingleton<ServiceBus>();
					services.AddSingleton<ServiceBusAsync>();

					services.AddSingleton<ILogger>(CreateLogger());
					services.AddSingleton<IWorkflowEngine, WorkflowEngine>();

					services.AddSingleton<ISagaKeeper, SqlServerSagaKeeper>();

					services.AddSingleton<ResourceManager>();
					services.AddSingleton<SagaManager>();
					services.AddSingleton<PluginManager>();

					services.AddSingleton<IResourceManager>(svc => svc.GetService<ResourceManager>());
					services.AddSingleton<IResourceWrapper>(svc => svc.GetService<ResourceManager>());
					services.AddSingleton<ISagaManager, SagaManager>();
					services.AddSingleton<IPluginManager, PluginManager>();
					services.AddSingleton<IServiceBus, ServiceBusAsync>();

					services.AddSingleton(svc => svc.GetService<ISagaManager>().Resolver);

					services.AddSingleton<Api.ProcessApi>();
				})
				.ConfigureWebHostDefaults(webBuilder =>
				{
					webBuilder.UseStartup<Startup>();
				});

		

		
	}
}
