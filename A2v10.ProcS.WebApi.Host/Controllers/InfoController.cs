// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Dynamic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using A2v10.ProcS.Api;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using A2v10.ProcS.Infrastructure;

namespace A2v10.ProcS.WebApi.Host.Controllers
{
	[JsonObject]
	public class Info
	{
		[JsonProperty("status")]
		public String Status => "ok";
	}
	
	[ApiController]
	[Route("[controller]")]
	public class InfoController : ControllerBase
	{
		[HttpGet]
		public Info Get()
		{
			return new Info();
		}
	}
}