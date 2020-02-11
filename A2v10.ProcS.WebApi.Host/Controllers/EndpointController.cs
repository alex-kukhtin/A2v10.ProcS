// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;
using System.Dynamic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using A2v10.ProcS.Infrastructure;
using Microsoft.AspNetCore.Authorization;

namespace A2v10.ProcS.WebApi.Host.Controllers
{
	[Route("[controller]")]
	[ApiController]
	public class EndpointsController : ControllerBase
	{
		private readonly IEndpointResolver _endointManager;

		public EndpointsController(IEndpointResolver endointManager)
		{
			_endointManager = endointManager;
		}

		[HttpPost]
		//[Authorize]
		[Route("{key}/{*extra}")]
		public async Task Handle([FromBody] String body, String key, String extra)
		{
			var handler = _endointManager.GetHandler(key);
			var ret = await handler.HandleAsync(body, extra);
			Content(ret.body, ret.type);
		}
	}
}