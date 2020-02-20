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
		public const string ResName = "com.a2.procs";
		
		public static void RegisterSagas(IResourceManager resourceManager, ISagaManager sagaManager)
		{
			{
				var fact = new ConstructSagaFactory<BookmarkSaga>(BookmarkSaga.ukey);
				resourceManager.RegisterResourceFactory(fact.SagaKind, new SagaResourceFactory(fact));
				sagaManager.RegisterSagaFactory<SetBookmarkMessage, ResumeBookmarkMessage>(fact);
				//resourceManager.RegisterResources<SetBookmarkMessage, ResumeBookmarkMessage>();
			}
			{
				var fact = new ConstructSagaFactory<ProcessSaga>(ProcessSaga.ukey);
				resourceManager.RegisterResourceFactory(fact.SagaKind, new SagaResourceFactory(fact));
				sagaManager.RegisterSagaFactory<StartProcessMessage, ContinueActivityMessage>(fact);
				//resourceManager.RegisterResources<StartProcessMessage, ContinueActivityMessage>();
			}
			{
				var fact = new ConstructSagaFactory<CallHttpApiSaga>(CallHttpApiSaga.ukey);
				resourceManager.RegisterResourceFactory(fact.SagaKind, new SagaResourceFactory(fact));
				sagaManager.RegisterSagaFactory<CallApiRequestMessage, CallApiResponseMessage>(fact);
				//resourceManager.RegisterResources<CallApiRequestMessage, CallApiResponseMessage>();
			}
			{
				var fact = new ConstructSagaFactory<RegisterCallbackSaga>(RegisterCallbackSaga.ukey);
				resourceManager.RegisterResourceFactory(fact.SagaKind, new SagaResourceFactory(fact));
				sagaManager.RegisterSagaFactory<RegisterCallbackMessage, CallbackMessage>(fact);
				//resourceManager.RegisterResources<RegisterCallbackMessage, CallbackMessage>();
			}
			{
				var fact = new ConstructSagaFactory<CallbackCorrelationSaga>(CallbackCorrelationSaga.ukey);
				resourceManager.RegisterResourceFactory(fact.SagaKind, new SagaResourceFactory(fact));
				sagaManager.RegisterSagaFactory<WaitCallbackMessage, CorrelatedCallbackMessage>(fact);
				//resourceManager.RegisterResources<WaitCallbackMessage, CorrelatedCallbackMessage>();
			}
		}

		public static void RegisterActivities(IResourceManager resourceManager)
		{

		}
	}
}
