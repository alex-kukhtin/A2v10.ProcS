using A2v10.ProcS.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;

namespace A2v10.ProcS
{
	public class InstanceWrapper
	{
		private readonly IInstance _instance;
		public InstanceWrapper(IInstance instance)
		{
			_instance = instance;
		}

		public String Id => _instance.Id.ToString();
		public String ParentId => _instance?.ParentInstanceId.ToString();
		public String CurrentState => _instance.CurrentState;
		public Boolean IsComplete => _instance.IsComplete;
	}
}
