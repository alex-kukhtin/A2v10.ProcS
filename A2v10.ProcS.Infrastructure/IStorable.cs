using System;
using System.Collections.Generic;
using System.Text;

namespace A2v10.ProcS.Infrastructure
{
	public interface IStorable
	{
		IDynamicObject Store(IResourceWrapper wrapper);
		void Restore(IDynamicObject store, IResourceWrapper wrapper);
	}
}
