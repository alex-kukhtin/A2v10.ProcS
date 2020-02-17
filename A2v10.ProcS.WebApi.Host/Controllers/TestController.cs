// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using A2v10.ProcS.Infrastructure;

namespace A2v10.ProcS.WebApi.Host.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class TestController : ControllerBase
	{
		private readonly IWorkflowStorage stor;
		public TestController(IWorkflowStorage stor)
		{
			this.stor = stor;
		}

		[HttpGet]
		public async Task Get()
		{
			var st = await stor.WorkflowFromStorage(new Identity("ChatBotExample.json"));
		}
	}
}
