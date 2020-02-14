// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Dynamic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using A2v10.ProcS.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace A2v10.ProcS.WebApi.Host.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class BusController : ControllerBase
	{
		private readonly IServiceBus bus;

		public BusController(IServiceBus bus)
		{
			this.bus = bus;
		}

		[HttpPost]
		//[Authorize]
		[Route("send")]
		public void SendMessage([FromBody] String json)
		{
			var sett = new JsonSerializerSettings()
			{
				TypeNameHandling = TypeNameHandling.Auto,
				TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple
			};
			var message = JsonConvert.DeserializeObject<IMessage>(json, sett);
			bus.Send(message);
		}
	}
}