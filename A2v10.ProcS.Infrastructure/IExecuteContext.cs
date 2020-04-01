// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace A2v10.ProcS.Infrastructure
{
	public interface IHandleContext 
	{
		IServiceBus Bus { get; }
		void SendMessage(IMessage message);
		void SendMessageAfter(DateTime after, IMessage message);
		void SendMessagesSequence(params IMessage[] messages);
		ILogger Logger { get; }
		INotifyManager NotifyManager { get; }
	}

	public interface IExecuteContext : IHandleContext
	{
		IInstance Instance { get; }
		Boolean IsContinue { get; set; }
		String Bookmark { get; set; }
		IDynamicObject Result { get; set; }

		Task SaveInstance();

		String Resolve(String source);
		DynamicObject Resolve(IDynamicObject source);
		T EvaluateScript<T>(String expression);
		IDynamicObject EvaluateScriptObject(String expression);
		void ExecuteScript(String code);

		void ProcessComplete(String bookmark);

		Guid SetBookmark();
		void ResumeBookmark(Guid id, IDynamicObject result);

		IExecuteContext CreateExecuteContext(IInstance instance, String bookmark = null, IDynamicObject result = null);
		Task<IInstance> StartProcess(String processId, Guid parentId, IDynamicObject data = null);

		void ContinueProcess(Guid id, String bookmark, IDynamicObject result);
		void ContinueProcess(Guid id, String bookmark, String json);
	}
}
