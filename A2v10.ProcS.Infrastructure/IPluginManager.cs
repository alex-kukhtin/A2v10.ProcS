// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace A2v10.ProcS.Infrastructure
{
	public interface IPluginManager
	{
		void RegisterResources(IResourceManager mgr);
		void RegisterSagas(IResourceManager rmgr, ISagaManager smgr);
	}
}
