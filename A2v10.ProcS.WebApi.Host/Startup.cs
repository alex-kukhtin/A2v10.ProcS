using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

			var storage = new Classes.FakeStorage(Configuration["ProcS:Workflows"]);

			services.AddSingleton<IEndpointManager, EndpointManager>();

			services.AddSingleton<IWorkflowStorage>(storage);
			services.AddSingleton<IInstanceStorage>(storage);

			services.AddSingleton<IScriptEngine, ScriptEngine>();
			services.AddSingleton<IRepository, Repository>();
			services.AddSingleton<IServiceBus, ServiceBus>();

			services.AddSingleton<IWorkflowEngine, WorkflowEngine>();

			services.AddSingleton<ISagaKeeper, InMemorySagaKeeper>();

			services.AddSingleton(CreateSagaManager);
		}

		public ISagaManager CreateSagaManager(IServiceProvider serviceProvider)
		{
			var mgr = new SagaManager(serviceProvider);

			mgr.RegisterSagaFactory<ResumeProcessMessage>(new ConstructSagaFactory<ProcessSaga>(nameof(ProcessSaga)));
			mgr.RegisterSagaFactory<StartProcessMessage>(new ConstructSagaFactory<ProcessSaga>(nameof(ProcessSaga)));

			mgr.RegisterSagaFactory<CallApiRequestMessage, CallApiResponse>(new ConstructSagaFactory<CallHttpApiSaga>(nameof(CallHttpApiSaga)));
			mgr.RegisterSagaFactory<WaitCallbackMessage, CallbackMessage>(new ConstructSagaFactory<WaitApiCallbackSaga>(nameof(WaitApiCallbackSaga)));
			mgr.RegisterSagaFactory<WaitCallbackMessageProcess, CallbackMessageResume>(new ConstructSagaFactory<WaitApiCallbackProcessSaga>(nameof(WaitApiCallbackProcessSaga)));

			foreach (var path in GetPluginPathes())
			{
				mgr.LoadPlugins(path, Configuration.GetSection("ProcS.Plugins"));
			}

			return mgr;
		}

		private IEnumerable<String> GetPluginPathes()
		{
			return Configuration.GetSection("ProcS:Plugins").GetChildren().Select(s => s.Value);
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
