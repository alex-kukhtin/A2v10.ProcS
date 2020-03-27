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
	public class StartProcessRequest : IStartProcessRequest
	{
		[JsonProperty("processId")]
		public String ProcessId { get; set; }
		[JsonProperty("parameters")]
		public IDynamicObject Parameters { get; set; }
	}

	[JsonObject]
	public class ResumeProcessRequest : IResumeProcessRequest
	{
		[JsonProperty("instanceId")]
		public Guid InstanceId { get; set; }
		[JsonProperty("bookmark")]
		public String Bookmark { get; set; }
		[JsonProperty("bookmark")]
		public IDynamicObject Result { get; set; }
	}

	[JsonObject]
	public class InstanceResponse
	{
		[JsonProperty("instanceId")]
		public Guid InstanceId { get; set; }
	}

	[Route("api/[controller]")]
	[ApiController]
	public class ProcessController : ControllerBase
	{
		private readonly IWorkflowEngine _engine;

		public ProcessController(IWorkflowEngine engine)
		{
			_engine = engine;
		}

		[HttpPost]
		//[Authorize]
		[Route("start")]
		public async Task<InstanceResponse> StartProcess([FromBody] StartProcessRequest prm)
		{
			var wf = await _engine.StartWorkflow(prm.ProcessId, DynamicObjectConverters.From(prm.Parameters));
			return new InstanceResponse()
			{
				InstanceId = wf.Id
			};
		}

		[HttpPost]
		//[Authorize]
		[Route("resume")]
		public async Task Resume([FromBody] ResumeProcessRequest prm)
		{
			await _engine.ResumeWorkflow(prm.InstanceId, prm.Bookmark, DynamicObjectConverters.From(prm.Result));
		}
	}
}