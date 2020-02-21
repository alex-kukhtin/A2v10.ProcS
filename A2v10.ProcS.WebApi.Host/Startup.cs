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
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddControllers(SetControllerOptions);
			services.AddAuthentication(SetAuthenticationOptions).AddJwtBearer(SetJwtBearerOptions);

			services.AddMvc(opt =>
			{
				opt.InputFormatters.Insert(0, new MvcExtensions.RawJsonBodyInputFormatter());
			});

			var tm = new Classes.TaskManager();

			services.AddSingleton<ITaskManager>(tm);

			var storage = new Classes.FakeStorage(Configuration["ProcS:Workflows"]);

			var epm = new EndpointManager();

			services.AddSingleton<IEndpointManager>(epm);
			services.AddSingleton<IEndpointResolver>(epm);

			services.AddSingleton<IWorkflowStorage>(storage);
			services.AddSingleton<IInstanceStorage>(storage);

			services.AddSingleton<IScriptEngine, ScriptEngine>();
			services.AddSingleton<IRepository, Repository>();
			services.AddSingleton<InMemoryServiceBus>();
			services.AddSingleton(CreateServiceBus);

			services.AddSingleton<IWorkflowEngine, WorkflowEngine>();

			services.AddSingleton<ISagaKeeper, InMemorySagaKeeper>();

			services.AddSingleton<PluginManager>();
			services.AddSingleton(CreatePluginManager);

			services.AddSingleton<ResourceManager>();
			services.AddSingleton(CreateResourceManager);

			services.AddSingleton<SagaManager>();
			services.AddSingleton(CreateSagaManager);

			services.AddSingleton(svs => svs.GetService<ISagaManager>().Resolver);
		}

		private static IServiceBus CreateServiceBus(IServiceProvider serviceProvider)
		{
			var bus = serviceProvider.GetService<InMemoryServiceBus>();
			new Thread(bus.Start).Start();
			return bus;
		}

		private ISagaManager CreateSagaManager(IServiceProvider serviceProvider)
		{
			var mgr = serviceProvider.GetService<SagaManager>();
			var rm = serviceProvider.GetService<IResourceManager>();
			ProcS.RegisterSagas(rm, mgr);
			var pl = serviceProvider.GetService<IPluginManager>();
			pl.RegisterResources(rm, mgr);

			return mgr;
		}

		private IResourceManager CreateResourceManager(IServiceProvider serviceProvider)
		{
			var mgr = serviceProvider.GetService<ResourceManager>();

			ProcS.RegisterActivities(mgr);

			return mgr;
		}

		private IPluginManager CreatePluginManager(IServiceProvider serviceProvider)
		{
			var mgr = serviceProvider.GetService<PluginManager>();

			foreach (var path in GetPluginPathes())
			{
				mgr.LoadPlugins(path, Configuration.GetSection("ProcS:Plugins"));
			}

			return mgr;
		}

		private IEnumerable<String> GetPluginPathes()
		{
			return Configuration.GetSection("ProcS:PluginsPath").GetChildren().Select(s => s.Value);
		}

		public static void SetControllerOptions(MvcOptions options)
		{
			options.EnableEndpointRouting = false;
		}

		public void SetAuthenticationOptions(AuthenticationOptions options)
		{

		}

		public void SetJwtBearerOptions(JwtBearerOptions options)
		{
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseRouting();

			app.UseAuthorization();
			app.UseAuthentication();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
		}
	}
}
