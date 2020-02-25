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

namespace A2v10.ProcS.WebApi.Host
{
	using Host = Microsoft.Extensions.Hosting.Host;

	public static class Program
	{

		public static void Main(String[] args)
		{
			CreateHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateHostBuilder(String[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureServices((ctx, services) =>
				{
					var conf = ctx.Configuration;

					services.AddHostedService<Service>();

					var tm = new Classes.TaskManager();

					services.AddSingleton<ITaskManager>(tm);

					var storage = new Classes.FakeStorage(conf["ProcS:Workflows"]);

					var epm = new EndpointManager();

					services.AddSingleton<IEndpointManager>(epm);
					services.AddSingleton<IEndpointResolver>(epm);

					services.AddSingleton<IWorkflowStorage>(storage);
					services.AddSingleton<IInstanceStorage>(storage);

					services.AddSingleton<IScriptEngine, ScriptEngine>();
					services.AddSingleton<IRepository, Repository>();
					services.AddSingleton<ServiceBus>();
					services.AddSingleton<ServiceBusAsync>();

					services.AddSingleton<IWorkflowEngine, WorkflowEngine>();

					services.AddSingleton<ISagaKeeper, InMemorySagaKeeper>();

					services.AddSingleton<ResourceManager>();
					services.AddSingleton<SagaManager>();
					services.AddSingleton<PluginManager>();

					services.AddSingleton<IResourceManager, ResourceManager>();
					services.AddSingleton<ISagaManager, SagaManager>();
					services.AddSingleton<IPluginManager, PluginManager>();
					services.AddSingleton<IServiceBus, ServiceBusAsync>();

					services.AddSingleton(svc => svc.GetService<ISagaManager>().Resolver);
				})
				.ConfigureWebHostDefaults(webBuilder =>
				{
					webBuilder.UseStartup<Startup>();
				});

		

		
	}
}
