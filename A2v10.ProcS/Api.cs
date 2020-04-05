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

	public enum Status
	{
		ok,
		error,
		timeout
	}

	public class ResumeResponse
	{
		public String Result { get; set; }
		public Status Status { get; set; }
		public String Message { get; set; }
		public IInstance Instance { get; set; }
	}

	public class ProcessApi
	{
		private readonly IWorkflowEngine _engine;
		private readonly INotifyManager _notifyManager;
		private readonly IRepository _repository;

		public ProcessApi(IWorkflowEngine engine, INotifyManager notifyManager, IRepository repository)
		{
			_engine = engine;
			_notifyManager = notifyManager;
			_repository = repository;
		}

		public Task<IInstance> StartProcess(IStartProcessRequest prm)
		{
			return _engine.StartWorkflow(prm.ProcessId, prm.Parameters);
		}

		public async Task<ResumeResponse> Resume(IResumeProcessRequest prm)
		{
			var result = new ResumeResponse();
			try
			{
				var promise = new Promise<String>();
				_notifyManager.Register(prm.InstanceId, promise);
				await _engine.ResumeBookmark(prm.InstanceId, prm.Bookmark, prm.Result);
				var t1 = promise.WaitFor<String>();
				var t2 = Task.Delay(5000);
				var tr = await Task.WhenAny(t1, t2);
				if (tr == t1)
				{
					result.Status = Status.ok;
					result.Result = t1.Result;
					result.Instance = await _repository.Get(prm.InstanceId);
				}
				else if (tr == t2)
					result.Status = Status.timeout;
			} 
			catch (Exception ex)
			{
				if (ex.InnerException != null)
					ex = ex.InnerException;
				result.Status = Status.error;
				result.Message = ex.Message;
			}
			return result;
		}
	}
}
