using A2v10.ProcS.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace A2v10.ProcS
{
	public struct Identity : IIdentity
	{
		public String ProcessId { get; }
		public Int32 Version { get; }

		public Identity(String processId, Int32 version = 0)
		{
			ProcessId = processId;
			Version = version;
		}
	}
}
