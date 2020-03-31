
using A2v10.Data.Interfaces;
using A2v10.ProcS.Infrastructure;
using System;

namespace A2v10.ProcS.SqlServer
{
	public static class SqlServerProcS
	{
		public const String ResName = "com.a2v10.procs.sqlserver";

		public static void RegisterSagas(IResourceManager resourceManager, ISagaManager sagaManager, IDbContext dbContext)
		{
			{
				var fact = new DelegateSagaFactory(ExecuteSqlSaga.ukey, () => new ExecuteSqlSaga(dbContext));
				resourceManager.RegisterResourceFactory(fact.SagaKind, new SagaResourceFactory(fact));
				sagaManager.RegisterSagaFactory<ExecuteSqlMessage>(fact);
				resourceManager.RegisterResources(typeof(ExecuteSqlMessage));
			}
		}

		public static void RegisterActivities(IResourceManager resourceManager)
		{
			resourceManager.RegisterResource<ExecuteSqlActivity>();
		}
	}
}
