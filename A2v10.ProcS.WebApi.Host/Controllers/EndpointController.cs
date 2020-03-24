// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

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
		private readonly IEndpointResolver _endpointManager;

		public EndpointsController(IEndpointResolver endpointManager)
		{
			_endpointManager = endpointManager;
		}

		[HttpPost]
		//[Authorize]
		[Route("{key}/{*extra}")]
		public async Task<IActionResult> Handle([FromBody] String body, String key, String extra)
		{
			var handler = _endpointManager.GetHandler(key);
			if (handler == null) return NotFound();
			var ret = await handler.HandleAsync(body, extra);
			return Content(ret.body, ret.type);
		}
	}
}