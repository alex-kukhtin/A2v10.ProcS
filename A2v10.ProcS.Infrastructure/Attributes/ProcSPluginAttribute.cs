using System;
using System.Collections.Generic;
using System.Text;

namespace A2v10.ProcS.Infrastructure
{
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]

	public sealed class ProcSPluginAttribute : Attribute
	{
	}
}
