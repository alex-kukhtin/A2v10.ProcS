// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using A2v10.ProcS.Infrastructure;

namespace A2v10.ProcS.SqlServer
{
	[ResourceKey(SqlServerProcS.ResName + ":" + nameof(ExecuteSqlActivity))]
	public class ExecuteSqlActivity : IActivity
	{
		public String DataSource { get; set; }
		public String Procedure { get; set; }
		public DynamicObject Parameters { get; set; }

		public ActivityExecutionResult Execute(IExecuteContext context)
		{	
			var m = new ExecuteSqlMessage(context.Instance.Id)
			{
				DataSource = context.Resolve(DataSource),
				Procedure = context.Resolve(Procedure),
				Parameters = context.Resolve(Parameters)
			};

			context.SendMessage(m);
			return ActivityExecutionResult.Complete;
		}
	}
}
