// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Threading.Tasks;
using A2v10.ProcS.Infrastructure;

namespace A2v10.ProcS.Api
{
	public interface IStartProcessRequest
	{
		String ProcessId { get; }
		IDynamicObject Parameters { get; }
	}

	public interface IResumeProcessRequest
	{
		Guid InstanceId { get; }
		String Bookmark { get; }
		IDynamicObject Result { get; }
	}

	public class ProcessApi
	{
		private readonly IWorkflowEngine _engine;

		public ProcessApi(IWorkflowEngine engine)
		{
			_engine = engine;
		}

		public Task<IInstance> StartProcess(IStartProcessRequest prm)
		{
			return _engine.StartWorkflow(prm.ProcessId, prm.Parameters);
		}

		public Task Resume(IResumeProcessRequest prm)
		{
			return _engine.ResumeBookmark(prm.InstanceId, prm.Bookmark, prm.Result);
		}
	}
}
