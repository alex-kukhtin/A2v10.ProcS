// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;

namespace A2v10.ProcS.Infrastructure
{
	public interface IIdentity
	{
		String ProcessId { get; }
		Int32 Version { get; }
	}
}
