// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using A2v10.ProcS.Infrastructure;

namespace A2v10.ProcS
{
	public static class ProcS
	{
		public const String ResName = "com.a2v10.procs";
		
		public static void RegisterSagas(IResourceManager resourceManager, ISagaManager sagaManager, IScriptEngine scriptEngine, IRepository repository)
		{
			{
				var fact = new ConstructSagaFactory<BookmarkSaga>(BookmarkSaga.ukey);
				resourceManager.RegisterResourceFactory(fact.SagaKind, new SagaResourceFactory(fact));
				sagaManager.RegisterSagaFactory<SetBookmarkMessage, ResumeBookmarkMessage>(fact);
				resourceManager.RegisterResources(typeof(SetBookmarkMessage), typeof(ResumeBookmarkMessage));
			}
			{
				var fact = new DelegateSagaFactory(ProcessSaga.ukey, () => new ProcessSaga(repository, scriptEngine));
				resourceManager.RegisterResourceFactory(fact.SagaKind, new SagaResourceFactory(fact));
				sagaManager.RegisterSagaFactory<StartProcessMessage, ContinueActivityMessage>(fact);
				resourceManager.RegisterResources(typeof(StartProcessMessage), typeof(ContinueActivityMessage));
			}
			{
				var fact = new ConstructSagaFactory<WaitResumeSaga>(WaitResumeSaga.ukey);
				resourceManager.RegisterResourceFactory(fact.SagaKind, new SagaResourceFactory(fact));
				sagaManager.RegisterSagaFactory<WaitResumeMessage, ResumeMessage>(fact);
				resourceManager.RegisterResources(typeof(WaitResumeMessage), typeof(ResumeMessage));
			}
			{
				var fact = new ConstructSagaFactory<CallHttpApiSaga>(CallHttpApiSaga.ukey);
				resourceManager.RegisterResourceFactory(fact.SagaKind, new SagaResourceFactory(fact));
				sagaManager.RegisterSagaFactory<CallApiRequestMessage, CallApiResponseMessage>(fact);
				resourceManager.RegisterResources(typeof(CallApiRequestMessage), typeof(CallApiResponseMessage));
			}
			{
				var fact = new DelegateSagaFactory(RegisterCallbackSaga.ukey, () => new RegisterCallbackSaga(scriptEngine));
				resourceManager.RegisterResourceFactory(fact.SagaKind, new SagaResourceFactory(fact));
				sagaManager.RegisterSagaFactory<RegisterCallbackMessage, CallbackMessage>(fact);
				resourceManager.RegisterResources(typeof(RegisterCallbackMessage), typeof(CallbackMessage));
			}
			{
				var fact = new ConstructSagaFactory<CallbackCorrelationSaga>(CallbackCorrelationSaga.ukey);
				resourceManager.RegisterResourceFactory(fact.SagaKind, new SagaResourceFactory(fact));
				sagaManager.RegisterSagaFactory<WaitCallbackMessage, CorrelatedCallbackMessage>(fact);
				resourceManager.RegisterResources(typeof(WaitCallbackMessage), typeof(CorrelatedCallbackMessage));
			}
		}

		public static void RegisterActivities(IResourceManager resourceManager)
		{
			resourceManager.RegisterResource<CallHttpApiActivity>();
			resourceManager.RegisterResource<CodeActivity>();
			resourceManager.RegisterResource<DelayActivity>();
			resourceManager.RegisterResource<SequenceActivity>();
			resourceManager.RegisterResource<ParallelActivity>();
			resourceManager.RegisterResource<StartProcessActivity>();
			resourceManager.RegisterResource<WaitResumeActivity>();
			resourceManager.RegisterResource<WaitCallbackActivity>();
			resourceManager.RegisterResource<SendCallbackActivity>();
		}
	}
}
