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
	public class Request
	{

	}

	[JsonObject]
	public class StartProcessRequest : Request, IStartProcessRequest
	{
		[JsonProperty("processId")]
		public String ProcessId { get; set; }
		[JsonProperty("parameters")]
		public IDynamicObject Parameters { get; set; }
	}

	[JsonObject]
	public class ResumeProcessRequest : Request, IResumeProcessRequest
	{
		[JsonProperty("instanceId")]
		public Guid InstanceId { get; set; }
		[JsonProperty("bookmark")]
		public String Bookmark { get; set; }
		[JsonProperty("result")]
		public IDynamicObject Result { get; set; }
	}

	[JsonObject]
	public class Response
	{
	}

	[JsonObject]
	public class InstanceResponse : Response
	{
		[JsonProperty("instanceId")]
		public Guid InstanceId { get; set; }
	}

	[JsonObject]
	public class ResumeResponse: Response
	{
		[JsonProperty("status")]
		public Status Status { get; set; }
		[JsonProperty("message")]
		public String Message { get; set; }
		[JsonProperty("result")]
		public String Result { get; set; }
	}

	[Route("api/[controller]")]
	[ApiController]
	public class ProcessController : ControllerBase
	{
		private readonly ProcessApi _api;

		public ProcessController(ProcessApi api)
		{
			_api = api;
		}

		[HttpPost]
		//[Authorize]
		[Route("start")]
		public async Task<Response> StartProcess([FromBody] StartProcessRequest prm)
		{
			var wf = await _api.StartProcess(prm);
			return new InstanceResponse()
			{
				InstanceId = wf.Id
			};
		}

		[HttpPost]
		//[Authorize]
		[Route("resume")]
		public async Task<ResumeResponse> Resume([FromBody] ResumeProcessRequest prm)
		{
			var r = await _api.Resume(prm);
			return new ResumeResponse()
			{
				Status = r.Status,
				Message = r.Message,
				Result = r.Result
			};
		}
	}
}