// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using A2v10.ProcS.Interfaces;

namespace A2v10.ProcS
{
	public class HandleContext : IHandleContext
	{
		protected readonly IServiceBus _serviceBus;
		protected readonly IInstanceStorage _instanceStorage;
		protected readonly IScriptEngine _scriptEngine;

		public HandleContext(IServiceBus bus, IInstanceStorage storage)
		{
			_serviceBus = bus;
			_instanceStorage = storage;
			_scriptEngine = new ScriptEngine();
		}

		public Task<IInstance> LoadInstance(Guid id)
		{
			return _instanceStorage.Load(id);
		}

		public void SendMessage(IMessage message)
		{
			_serviceBus.Send(message);
		}

		public IResumeContext CreateResumeContext(IInstance instance)
		{
			return new ResumeContext(_serviceBus, _instanceStorage, instance);
		}
	}

	public class ExecuteContext : HandleContext, IExecuteContext
	{
		public IInstance Instance { get; }

		public ExecuteContext(IServiceBus bus, IInstanceStorage storage, IInstance instance)
			: base(bus, storage)
		{
			Instance = instance;
			_scriptEngine.SetValue("params", Instance.GetParameters());
			_scriptEngine.SetValue("data", Instance.GetData());
			_scriptEngine.SetValue("result", Instance.GetResult());
		}

		public async Task SaveInstance()
		{
			await _instanceStorage.Save(Instance);
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
				String val = _scriptEngine.Eval<String>(key);
				sb.Replace(m.Value, val);
			}
			return sb.ToString();
		}

		public T EvaluateScript<T>(String expression)
		{
			return _scriptEngine.Eval<T>(expression);
		}

		public void ExecuteScript(String expression)
		{
			_scriptEngine.Execute(expression);
		}
	}

	public class ResumeContext : ExecuteContext, IResumeContext
	{
		public String Bookmark { get; set; }
		public String Result { get; set; }

		public ResumeContext(IServiceBus bus, IInstanceStorage storage, IInstance instance)
			: base(bus, storage, instance)
		{
		}
	}
}
