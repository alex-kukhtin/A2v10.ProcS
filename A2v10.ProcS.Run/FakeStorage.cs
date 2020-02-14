// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Threading.Tasks;
using A2v10.ProcS.Infrastructure;

namespace A2v10.ProcS.Run
{
	public class FakeStorage : IInstanceStorage
	{
		public IInstance Create(Guid processId)
		{
			throw new NotImplementedException(nameof(Create));
		}

		public Task<IInstance> Load(Guid instanceId)
		{
			throw new NotImplementedException(nameof(Load));
		}

		public Task Save(IInstance instance)
		{
			throw new NotImplementedException(nameof(Save));
		}

		IWorkflowDefinition FromString(String source)
		{
			throw new NotImplementedException(nameof(FromString));
		}
	}
}
