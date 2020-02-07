﻿// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;
using System.Dynamic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using A2v10.ProcS.Infrastructure;
using Microsoft.AspNetCore.Authorization;

namespace A2v10.ProcS.WebApi.Host.Controllers
{
	public class StartProcessRequest
	{
		public String ProcessId { get; set; }
		public ExpandoObject Parameters { get; set; }
	}

	public class ResumeProcessRequest
	{
		public Guid InstanceId { get; set; }
		public String Bookmark { get; set; }
		public String Result { get; set; }
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
		[Authorize]
		[Route("start")]
		public async Task StartProcess([FromBody] StartProcessRequest prm)
		{
			var result = await _engine.StartWorkflow(prm.ProcessId, prm.Parameters);
		}

		[HttpPost]
		[Authorize]
		[Route("resume")]
		public async Task Resume([FromBody] ResumeProcessRequest prm)
		{
			var instance = await _engine.ResumeWorkflow(prm.InstanceId, prm.Bookmark, prm.Result);
		}
	}
}