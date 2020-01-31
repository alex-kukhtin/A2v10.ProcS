using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace A2v10.ProcS
{
	public class Delay : WorkflowAction
	{
		public String Duration { get; set; }

		public override void Execute(ExecuteContext context)
		{
			TimeSpan span = TimeSpan.Parse(Duration);
			String bookmark = Guid.NewGuid().ToString();
			context.ScheduleAction(bookmark, DoAsync());
		}

		async Task<DynamicObject> DoAsync()
		{
			await Task.Delay(1000);
			var r = new DynamicObject();
			r.Set("Result", 42);
			return r;
		}
	}
}
