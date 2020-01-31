using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using A2v10.ProcS.Interfaces;

namespace A2v10.ProcS.Run
{
	public class FakeStorage : IInstanceStorage
	{
		public IWorkflowInstance Create(String processId)
		{
			throw new NotImplementedException(nameof(Create));
		}

		public IWorkflowInstance Load(String instanceId)
		{
			throw new NotImplementedException(nameof(Load));
		}

		public void Save(IWorkflowInstance instance)
		{
			throw new NotImplementedException(nameof(Save));
		}

		IWorkflowDefinition FromString(String source)
		{
			throw new NotImplementedException(nameof(FromString));
		}
	}
}
