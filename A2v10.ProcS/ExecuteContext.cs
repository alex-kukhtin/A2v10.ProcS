// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using A2v10.ProcS.Infrastructure;
using Microsoft.Extensions.Logging;

namespace A2v10.ProcS
{
	public class HandleContext : IHandleContext
	{
		protected readonly IServiceBus _serviceBus;
		protected readonly ILogger _logger;
		protected readonly INotifyManager _notifyManager;


		public HandleContext(IServiceBus bus, ILogger logger, INotifyManager notifyManager)
		{
			_serviceBus = bus ?? throw new ArgumentNullException(nameof(bus));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_notifyManager = notifyManager ?? throw new ArgumentNullException(nameof(notifyManager));
		}

		public ILogger Logger => _logger;
		public IServiceBus Bus => _serviceBus;
		public INotifyManager NotifyManager => _notifyManager;

		public void SendMessage(IMessage message)
		{
			_serviceBus.Send(message);
		}

		public void SendMessageAfter(DateTime after, IMessage message)
		{
			_serviceBus.SendAfter(after, message);
		}

		public void SendMessagesSequence(params IMessage[] messages)
		{
			_serviceBus.SendSequence(messages);
		}
	}

	public class ExecuteContext : HandleContext, IExecuteContext
	{
		protected readonly IRepository _repository;
		protected readonly IScriptContext _scriptContext;

		public IInstance Instance { get; }
		public Boolean IsContinue { get; set; }

		public String Bookmark { get; set; }
		public IDynamicObject Result { get; set; }

		public ExecuteContext(IServiceBus bus, IRepository repository, IScriptContext scriptContext, ILogger logger, INotifyManager notify, IInstance instance)
			: base (bus, logger, notify)
		{
			Instance = instance;
			_repository = repository ?? throw new ArgumentNullException(nameof(repository));
			_scriptContext = scriptContext ?? throw new ArgumentNullException(nameof(scriptContext));
			_scriptContext.SetValue("params", Instance.GetParameters());
			_scriptContext.SetValue("data", Instance.GetData());
			_scriptContext.SetValue("result", Instance.GetResult());
			_scriptContext.SetValue("instance", Instance.GetSelf());
			_scriptContext.SetValue("env", Instance.GetEnvironment());
		}

		public async Task SaveInstance()
		{
			await _repository.Save(Instance);
			_notifyManager.GetAndRemove(Instance.Id)?.SignalDone(Instance.CurrentState);
		}

		private Regex _regex = null;

		public String Resolve(String source)
		{
			if (source == null)
				return source;
			if (_regex == null)
				_regex = new Regex("\\{\\{(.+?)\\}\\}", RegexOptions.Compiled);
			var ms = _regex.Matches(source);
			if (ms.Count == 0)
				return source;
			var sb = new StringBuilder(source);
			foreach (Match m in ms)
			{
				String key = m.Groups[1].Value;
				Object val = _scriptContext.Eval(key);
				sb.Replace(m.Value, val?.ToString());
			}
			return sb.ToString();
		}

		public DynamicObject Resolve(IDynamicObject source)
		{
			var result = new DynamicObject();
			foreach (var p in source)
			{
				if (p.Value is String strVal)
					result.Add(p.Key, Resolve(strVal));
				else
					result.Add(p.Key, p.Value);
			}
			return result;
		}



		public T EvaluateScript<T>(String expression)
		{
			if (expression == null)
				return default;
			return _scriptContext.Eval<T>($"({expression})");
		}

		public Object EvaluateScript(String expression)
		{
			if (expression == null)
				return null;
			return _scriptContext.Eval($"({expression})");
		}

		public IDynamicObject EvaluateScriptObject(String expression)
		{
			if (expression == null)
				return null;
			return _scriptContext.EvalObject($"({expression})");
		}

		public void ExecuteScript(String code)
		{
			_scriptContext.Execute(code);
		}

		public void ProcessComplete(String bookmark)
		{
			if (Instance.ParentInstanceId == Guid.Empty)
				return;
			var msg = new ContinueActivityMessage(Instance.ParentInstanceId, bookmark, Instance.GetResult());
			_serviceBus.Send(msg);
		}

		public Guid SetBookmark()
		{
			var id = Guid.NewGuid();
			var msg = new SetBookmarkMessage(id, new ContinueActivityMessage(Instance.Id, String.Empty));
			_serviceBus.Send(msg);
			return id;
		}

		public void ResumeBookmark(Guid id, IDynamicObject result)
		{
			if (id == Guid.Empty)
				throw new ArgumentOutOfRangeException("ExecuteContext.ResumeBookmark. Bookmark is empty");
			var msg = new ResumeBookmarkMessage(id, result);
			_serviceBus.Send(msg);
		}

		public IExecuteContext CreateExecuteContext(IInstance instance, String bookmark = null, IDynamicObject result = null)
		{
			return new ExecuteContext(_serviceBus, _repository, _scriptContext, _logger, _notifyManager, instance)
			{
				Bookmark = bookmark,
				Result = result
			};
		}

		public void ContinueProcess(Guid id, String bookmark, String json)
		{
			ContinueProcess(id, bookmark, DynamicObjectConverters.FromJson(json));
		}

		public void ContinueProcess(Guid id, String bookmark, IDynamicObject result)
		{
			var msg = new ContinueActivityMessage(id, bookmark, result);
			SendMessage(msg);
		}

		public async Task<IInstance> StartProcess(String processId, Guid parentId, IDynamicObject data = null)
		{
			var instance = await _repository.CreateInstance(new Identity(processId), parentId);
			if (data != null)
				instance.SetParameters(data);
			using (var newScriptContext = _scriptContext.NewContext())
			{
				var context = new ExecuteContext(_serviceBus, _repository, newScriptContext, _logger, _notifyManager, instance);
				await instance.Workflow.Run(context);
				return instance;
			}
		}
	}
}
