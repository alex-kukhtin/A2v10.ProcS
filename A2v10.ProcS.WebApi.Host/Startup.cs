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
using Newtonsoft.Json.Converters;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Serialization;

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
			services.AddControllers(SetControllerOptions).AddNewtonsoftJson(ConfigureNewtonsoft);
			services
				.AddAuthorization()
				.AddAuthentication(SetAuthenticationOptions)
				.AddJwtBearer(SetJwtBearerOptions);

			services.AddMvc(ConfigureMvc).AddNewtonsoftJson( ConfigureNewtonsoft);
		}

		private static void ConfigureMvc(MvcOptions opt)
		{
			opt.InputFormatters.Insert(0, new MvcExtensions.RawJsonBodyInputFormatter());
		}

		private static void ConfigureNewtonsoft(MvcNewtonsoftJsonOptions opt)
		{
			opt.SerializerSettings.ContractResolver = new DefaultContractResolver();
			opt.SerializerSettings.Converters.Add(new StringEnumConverter());
			opt.SerializerSettings.Converters.Add(new ExpandoObjectConverter());
			opt.SerializerSettings.Converters.Add(new DynamicObjectConverter());
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
			options.RequireHttpsMetadata = false;
			options.TokenValidationParameters = new TokenValidationParameters
			{
				ValidateLifetime = true,
				ValidateIssuer = true,
				ValidateAudience = true,
				ValidIssuer = "VALID_ISSUER",
				ValidateIssuerSigningKey = true,
				ValidAudiences = new List<String>() { "Audience1", "Audience2" },
				//IssuerSigningKey = 
			};
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
				app.UseDeveloperExceptionPage();

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
