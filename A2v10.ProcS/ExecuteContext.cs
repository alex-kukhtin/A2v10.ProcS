// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using A2v10.ProcS.Infrastructure;

namespace A2v10.ProcS
{
	public class HandleContext : IHandleContext
	{
		protected readonly IServiceBus _serviceBus;
		protected readonly IRepository _repository;
		protected readonly IScriptContext _scriptContext;

		public HandleContext(IServiceBus bus, IRepository repository, IScriptContext scriptContext)
		{
			_serviceBus = bus ?? throw new ArgumentNullException(nameof(bus));
			_repository = repository ?? throw new ArgumentNullException(nameof(repository));
			_scriptContext = scriptContext ?? throw new ArgumentNullException(nameof(scriptContext));
		}

		public IScriptContext ScriptContext => _scriptContext; 

		public Task<IInstance> LoadInstance(Guid id)
		{
			return _repository.Get(id);
		}

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

		public IExecuteContext CreateExecuteContext(IInstance instance, String bookmark = null, IDynamicObject result = null)
		{
			return new ExecuteContext(_serviceBus, _repository, _scriptContext, instance)
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
				var context = new ExecuteContext(_serviceBus, _repository, newScriptContext, instance);
				await instance.Workflow.Run(context);
				return instance;
			}
		}

		public void ResumeBookmark(Guid id, IDynamicObject result)
		{
			if (id == Guid.Empty)
				throw new ArgumentOutOfRangeException("ExecuteContext.ResumeBookmark. Bookmark is empty");
			var msg = new ResumeBookmarkMessage(id, result);
			_serviceBus.Send(msg);
		}
	}

	public class ExecuteContext : HandleContext, IExecuteContext
	{
		public IInstance Instance { get; }
		public Boolean IsContinue { get; set; }

		public String Bookmark { get; set; }
		public IDynamicObject Result { get; set; }

		public ExecuteContext(IServiceBus bus, IRepository repository, IScriptContext scriptContext, IInstance instance)
			: base(bus, repository, scriptContext)
		{
			Instance = instance;
			_scriptContext.SetValue("params", Instance.GetParameters());
			_scriptContext.SetValue("data", Instance.GetData());
			_scriptContext.SetValue("result", Instance.GetResult());
		}

		public async Task SaveInstance()
		{
			await _repository.Save(Instance);
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
				String val = _scriptContext.Eval<String>(key);
				sb.Replace(m.Value, val);
			}
			return sb.ToString();
		}

		public T EvaluateScript<T>(String expression)
		{
			return _scriptContext.Eval<T>($"({expression})");
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
	}
}
